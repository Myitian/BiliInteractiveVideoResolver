using System.Linq.Expressions;
using System.Numerics;

namespace LibBiliInteractiveVideo.Execution.Compilation;

public struct Condition<T>(ConditionOperation op, T value) : IEquatable<Condition<T>>, IComparable<Condition<T>>, IEqualityOperators<Condition<T>, Condition<T>, bool>
    where T : INumberBase<T>, IComparable<T>, IComparisonOperators<T, T, bool>
{
    public ConditionOperation Op = op;
    public T Value = value;
    public readonly bool Test(T value) => Op switch
    {
        ConditionOperation.EQ => value == Value,
        ConditionOperation.LE => value <= Value,
        ConditionOperation.LT => value < Value,
        ConditionOperation.GE => value >= Value,
        _ => value > Value
    };
    public readonly BinaryExpression CreateExpression(Expression value) => Op switch
    {
        ConditionOperation.EQ => Expression.Equal(value, ExpressionValueCache<T>.GetConstantOrNew(Value)),
        ConditionOperation.LE => Expression.LessThanOrEqual(value, ExpressionValueCache<T>.GetConstantOrNew(Value)),
        ConditionOperation.LT => Expression.LessThan(value, ExpressionValueCache<T>.GetConstantOrNew(Value)),
        ConditionOperation.GE => Expression.GreaterThanOrEqual(value, ExpressionValueCache<T>.GetConstantOrNew(Value)),
        _ => Expression.GreaterThan(value, ExpressionValueCache<T>.GetConstantOrNew(Value))
    };
    public int CompareTo(Condition<T> other) => Op - other.Op is not 0 and int tmp ? tmp : Value.CompareTo(other.Value);
    public readonly bool Equals(Condition<T> other) => Op == other.Op && Value.Equals(other.Value);
    public override readonly bool Equals(object? obj) => obj is Condition<T> other && Equals(other);
    public override readonly int GetHashCode() => HashCode.Combine(Op, Value);
    public override readonly string ToString() => $"{{{Op} {Value}}}";
    public static bool operator ==(Condition<T> left, Condition<T> right) => left.Equals(right);
    public static bool operator !=(Condition<T> left, Condition<T> right) => !left.Equals(right);


    public static Expression CreateExpression(Expression value, (bool, Condition<T>?, Condition<T>?) conditions)
    {
        return CreateExpression(value, conditions.Item1, conditions.Item2, conditions.Item3);
    }
    public static Expression CreateExpression(Expression value, bool status, Condition<T>? condition1 = null, Condition<T>? condition2 = null)
    {
        if (status == false)
            return ExpressionCache.False;
        if (condition1.HasValue != condition2.HasValue)
            return (condition1 ?? condition2!.Value).CreateExpression(value);
        else
        {
            if (!condition1.HasValue)
                return ExpressionCache.True;
            return Expression.AndAlso(
                condition1!.Value.CreateExpression(value),
                condition2!.Value.CreateExpression(value));
        }
    }
    public static Expression CreateExpression(Expression value, params IEnumerable<Condition<T>> conditions)
        => CreateExpression<IEnumerable<Condition<T>>>(value, conditions);
    public static Expression CreateExpression<TEnumerable>(Expression value, scoped TEnumerable conditions)
        where TEnumerable : IEnumerable<Condition<T>>, allows ref struct
    {
        Expression? check = null;
        foreach (Condition<T> condition in conditions)
            check = check is null ?
                condition.CreateExpression(value) :
                Expression.AndAlso(check, condition.CreateExpression(value));
        return check ?? ExpressionCache.True;
    }
    public static (bool, Condition<T>?, Condition<T>?) SimplifyAnds(params IEnumerable<Condition<T>> conditions)
        => SimplifyAnds<IEnumerable<Condition<T>>>(conditions);
    public static (bool, Condition<T>?, Condition<T>?) SimplifyAnds<TEnumerable>(scoped TEnumerable conditions)
        where TEnumerable : IEnumerable<Condition<T>>, allows ref struct
    {
        if (conditions is null)
            return (true, null, null);

        T eqValue = default!;
        bool eqHasValue = false;
        T minValue = default!;
        bool minHasValue = false;
        bool minInclusive = false;
        T maxValue = default!;
        bool maxHasValue = false;
        bool maxInclusive = false;

        foreach (Condition<T> condition in conditions)
        {
            switch (condition.Op)
            {
                case ConditionOperation.EQ when eqHasValue
                    && eqValue != condition.Value:
                    goto ALWAYS_FALSE;
                case ConditionOperation.EQ:
                    eqHasValue = true;
                    eqValue = condition.Value;
                    break;
                case ConditionOperation.GT when !minHasValue
                    || condition.Value > minValue:
                    minHasValue = true;
                    minValue = condition.Value;
                    minInclusive = false;
                    break;
                case ConditionOperation.GE when !minHasValue
                    || condition.Value > minValue
                    || condition.Value == minValue && !minInclusive:
                    minHasValue = true;
                    minValue = condition.Value;
                    minInclusive = true;
                    break;
                case ConditionOperation.LT when !maxHasValue
                    || condition.Value < maxValue:
                    maxHasValue = true;
                    maxValue = condition.Value;
                    maxInclusive = false;
                    break;
                case ConditionOperation.LE when !maxHasValue
                    || condition.Value < maxValue
                    || condition.Value == maxValue && !maxInclusive:
                    maxHasValue = true;
                    maxValue = condition.Value;
                    maxInclusive = true;
                    break;
            }
        }
        if (eqHasValue)
        {
            if (minHasValue && (minValue > eqValue || minValue == eqValue && !minInclusive))
                goto ALWAYS_FALSE;
            if (maxHasValue && (maxValue < eqValue || maxValue == eqValue && !maxInclusive))
                goto ALWAYS_FALSE;
            return (true,
                new(ConditionOperation.EQ, eqValue),
                null);
        }
        else if (minHasValue)
        {
            if (!maxHasValue)
            {
                return (true,
                    new(minInclusive ? ConditionOperation.GE : ConditionOperation.GT, minValue),
                    null);
            }
            if (minValue == maxValue)
            {
                if (minInclusive && maxInclusive)
                    return (true, new(ConditionOperation.EQ, minValue), null);
                goto ALWAYS_FALSE;
            }
            if (minValue > maxValue)
                goto ALWAYS_FALSE;
            return (true,
                new(minInclusive ? ConditionOperation.GE : ConditionOperation.GT, minValue),
                new(maxInclusive ? ConditionOperation.LE : ConditionOperation.LT, maxValue));
        }
        else if (maxHasValue)
        {
            return (true,
                new(maxInclusive ? ConditionOperation.LE : ConditionOperation.LT, maxValue),
                null);
        }
        return (true, null, null);
    ALWAYS_FALSE:
        return (false, null, null);
    }
}
