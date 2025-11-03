namespace LibBiliInteractiveVideo.Execution.Compilation;

public class CompileCache<T>
{
    private readonly Dictionary<string, Func<T[], bool>?> _conditions;
    private readonly Dictionary<string, Func<T[], bool>?>.AlternateLookup<ReadOnlySpan<char>> _conditionsLookup;
    private readonly Dictionary<string, Func<T[], bool>?> _conditionsWithoutRandom;
    private readonly Dictionary<string, Func<T[], bool>?>.AlternateLookup<ReadOnlySpan<char>> _conditionsWithoutRandomLookup;
    private readonly Dictionary<string, Action<T[]>?> _nativeActions;
    private readonly Dictionary<string, Action<T[]>?>.AlternateLookup<ReadOnlySpan<char>> _nativeActionsLookup;

    public CompileCache()
    {
        _conditions = [];
        _conditionsLookup = _conditions.GetAlternateLookup<ReadOnlySpan<char>>();
        _conditionsWithoutRandom = [];
        _conditionsWithoutRandomLookup = _conditionsWithoutRandom.GetAlternateLookup<ReadOnlySpan<char>>();
        _nativeActions = [];
        _nativeActionsLookup = _nativeActions.GetAlternateLookup<ReadOnlySpan<char>>();
    }

    public Func<T[], bool>? GetConditionOrCompute(scoped ReadOnlySpan<char> condition, Func<ReadOnlySpan<char>, Func<T[], bool>?> supplier)
    {
        if (_conditionsLookup.TryGetValue(condition, out Func<T[], bool>? result))
            return result;
        return _conditionsLookup[condition] = supplier(condition);
    }
    public Func<T[], bool>? GetConditionWithoutRandomOrCompute(scoped ReadOnlySpan<char> condition, Func<ReadOnlySpan<char>, Func<T[], bool>?> supplier)
    {
        if (_conditionsWithoutRandomLookup.TryGetValue(condition, out Func<T[], bool>? result))
            return result;
        return _conditionsWithoutRandomLookup[condition] = supplier(condition);
    }
    public Action<T[]>? GetNativeActionOrCompute(scoped ReadOnlySpan<char> nativeAction, Func<ReadOnlySpan<char>, Action<T[]>?> supplier)
    {
        if (_nativeActionsLookup.TryGetValue(nativeAction, out Action<T[]>? result))
            return result;
        return _nativeActionsLookup[nativeAction] = supplier(nativeAction);
    }
    public void Clear()
    {
        _conditions.Clear();
        _conditionsWithoutRandom.Clear();
        _nativeActions.Clear();
    }
}
