using System.Buffers;
using System.Collections;
using System.Numerics;

namespace LibBiliInteractiveVideo.Execution.Compilation;

public ref struct NamedConditionEnumerator<T>(ReadOnlySpan<char> expression)
    : IEnumerator<NamedCondition<T>>, IGetEnumerator<NamedConditionEnumerator<T>, NamedCondition<T>>
    where T : INumberBase<T>, IComparable<T>, IComparisonOperators<T, T, bool>
{
    private static readonly SearchValues<char> Ops = SearchValues.Create("<=>");

    private ReadOnlySpan<char> _expression = expression;
    private NamedCondition<T> _current = default;
    public readonly NamedCondition<T> Current => _current;
    public bool MoveNext()
    {
        ReadOnlySpan<char> expr = _expression;
        if (expr.IsEmpty)
            return false;
        expr = expr.TrimStart();
        int opIndex = expr.IndexOfAny(Ops);
        if (opIndex < 0)
            goto FAILED;
        _current.Name = expr[..opIndex].TrimEnd();
        expr = expr[opIndex..];
        if (expr.Length < 2)
            goto FAILED;
        if (expr[1] is '=')
        {
            _current.Condition.Op = expr[0] switch
            {
                '=' => ConditionOperation.EQ,
                '<' => ConditionOperation.LE,
                _ => ConditionOperation.GE,
            };
            expr = expr[2..].TrimStart();
        }
        else
        {
            _current.Condition.Op = expr[0] is '<' ? ConditionOperation.LT : ConditionOperation.GT;
            expr = expr[1..].TrimStart();
        }
        int andIndex = expr.IndexOf('&');
        if (andIndex < 0)
        {
            if (!T.TryParse(expr.TrimEnd(), null, out T? value))
                goto FAILED;
            _current.Condition.Value = value;
            _expression = [];
        }
        else
        {
            if (!T.TryParse(expr[..andIndex].TrimEnd(), null, out T? value))
                goto FAILED;
            _current.Condition.Value = value;
            _expression = expr[andIndex..].TrimStart('&');
        }
        return true;
    FAILED:
        _expression = default;
        return false;
    }
    public readonly NamedConditionEnumerator<T> GetEnumerator() => this;
    public readonly void Dispose() { }
    public readonly void Reset() => throw new NotSupportedException();
    readonly object IEnumerator.Current => throw new NotSupportedException();
}
