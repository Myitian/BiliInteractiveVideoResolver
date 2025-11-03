using System.Numerics;

namespace LibBiliInteractiveVideo.Execution.Compilation;

public class VariableReferenceTableEntry<T> : Dictionary<string, int>
    where T : INumberBase<T>
{
    public readonly AlternateLookup<ReadOnlySpan<char>> AlternateLookup;
    public T Modifier = T.Zero;

    public VariableReferenceTableEntry()
    {
        AlternateLookup = GetAlternateLookup<ReadOnlySpan<char>>();
    }

    public void Increase(ReadOnlySpan<char> value, int count = 1)
    {
        if (AlternateLookup.TryGetValue(value, out int c))
            AlternateLookup[value] = c + count;
        else
            AlternateLookup[value] = count;
    }

    public void Increase(string value, int count = 1)
    {
        if (TryGetValue(value, out int c))
            this[value] = c + count;
        else
            this[value] = count;
    }

    public void MergeFrom(VariableReferenceTableEntry<T> entry)
    {
        foreach ((string key, int value) in entry)
            Increase(key, value);
        Modifier += entry.Modifier;
    }
}