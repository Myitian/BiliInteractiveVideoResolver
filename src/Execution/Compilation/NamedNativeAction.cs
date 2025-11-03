using System.Numerics;

namespace LibBiliInteractiveVideo.Execution.Compilation;

public ref struct NamedNativeAction<T>(ReadOnlySpan<char> store, ReadOnlySpan<char> load, NativeAction<T> nativeAction)
    where T : INumberBase<T>
{
    public ReadOnlySpan<char> Store = store;
    public ReadOnlySpan<char> Load = load;
    public NativeAction<T> NativeAction = nativeAction;

    public readonly void Deconstruct(out ReadOnlySpan<char> store, out ReadOnlySpan<char> load, out NativeAction<T> nativeAction)
    {
        store = Store;
        load = Load;
        nativeAction = NativeAction;
    }
    public override readonly string ToString() => NativeAction.Op switch
    {
        NativeActionOperation.Add => $"{{{Store} = {Load} + {NativeAction.Value}}}",
        NativeActionOperation.Subtract => $"{{{Store} = {Load} - {NativeAction.Value}}}",
        NativeActionOperation.Assign => $"{{{Store} = {NativeAction.Value}}}",
        _ => $"{{{Store} = {Load} {NativeAction.Op} {NativeAction.Value}}}",
    };


    public static Dictionary<string, VariableReferenceTableEntry<T>> FlattenSequence<TEnumerable>(scoped TEnumerable actions)
        where TEnumerable : IEnumerable<NamedNativeAction<T>>, allows ref struct
    {
        Dictionary<string, VariableReferenceTableEntry<T>> referenceTable = [];
        var lookup = referenceTable.GetAlternateLookup<ReadOnlySpan<char>>();
        foreach (NamedNativeAction<T> action in actions)
        {
            if (!lookup.TryGetValue(action.Store, out VariableReferenceTableEntry<T>? store))
                lookup[action.Store] = store = [];
            VariableReferenceTableEntry<T>? load;
            switch (action.NativeAction.Op)
            {
                case NativeActionOperation.Add:
                    if (lookup.TryGetValue(action.Load, out load))
                        store.MergeFrom(load);
                    else
                        store.Increase(action.Load);
                    store.Modifier += action.NativeAction.Value;
                    break;
                case NativeActionOperation.Subtract:
                    if (lookup.TryGetValue(action.Load, out load))
                        store.MergeFrom(load);
                    else
                        store.Increase(action.Load);
                    store.Modifier -= action.NativeAction.Value;
                    break;
                default:
                    store.Clear();
                    store.Modifier = action.NativeAction.Value;
                    break;
            }
        }
        return referenceTable;
    }
}
