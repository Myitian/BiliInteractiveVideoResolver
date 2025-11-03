using System.Linq.Expressions;
using System.Numerics;

namespace LibBiliInteractiveVideo.Execution.Compilation;

public struct NativeAction<T>(NativeActionOperation op, T value) : IEquatable<NativeAction<T>>, IEqualityOperators<NativeAction<T>, NativeAction<T>, bool>
    where T : INumberBase<T>
{
    public NativeActionOperation Op = op;
    public T Value = value;
    public readonly T Apply(T value) => Op switch
    {
        NativeActionOperation.Add => value + Value,
        NativeActionOperation.Subtract => value - Value,
        _ => Value
    };
    public readonly Expression CreateExpression(Expression value) => Op switch
    {
        NativeActionOperation.Add => Expression.Add(value, ExpressionValueCache<T>.GetConstantOrNew(Value)),
        NativeActionOperation.Subtract => Expression.Subtract(value, ExpressionValueCache<T>.GetConstantOrNew(Value)),
        _ => ExpressionValueCache<T>.GetConstantOrNew(Value)
    };
    public readonly BinaryExpression CreateExpression(Expression store, Expression load) => Expression.Assign(store, CreateExpression(load));
    public readonly bool Equals(NativeAction<T> other) => Op == other.Op && Value.Equals(other.Value);
    public override readonly bool Equals(object? obj) => obj is NativeAction<T> other && Equals(other);
    public override readonly int GetHashCode() => HashCode.Combine(Op, Value);
    public override readonly string ToString() => $"{{{Op} {Value}}}";
    public static bool operator ==(NativeAction<T> left, NativeAction<T> right) => left.Equals(right);
    public static bool operator !=(NativeAction<T> left, NativeAction<T> right) => !left.Equals(right);
}
