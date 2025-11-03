using System.Numerics;

namespace LibBiliInteractiveVideo.Execution.Compilation;

public ref struct NamedCondition<T>(ReadOnlySpan<char> name, Condition<T> condition)
    where T : INumberBase<T>, IComparable<T>, IComparisonOperators<T, T, bool>
{
    public ReadOnlySpan<char> Name = name;
    public Condition<T> Condition = condition;

    public readonly void Deconstruct(out ReadOnlySpan<char> name, out Condition<T> condition)
    {
        name = Name;
        condition = Condition;
    }
    public override readonly string ToString() => $"{{{Name} {Condition.Op} {Condition.Value}}}";
}