using System.Collections.Frozen;
using System.Linq.Expressions;
using System.Numerics;

namespace LibBiliInteractiveVideo.Execution.Compilation;

public static class ExpressionCache
{
    public static readonly ConstantExpression True = Expression.Constant(true);
    public static readonly ConstantExpression False = Expression.Constant(false);
    public static readonly DefaultExpression NoOp = Expression.Empty();
}
public static class ExpressionCache<T>
{
    public static readonly ParameterExpression Array = Expression.Parameter(typeof(T[]), "array");
    // arrays are mutable
    internal static readonly ParameterExpression[] ArrayParams = [Array];
    public static readonly Func<T[], bool> AlwaysTrue = delegate { return true; };
    public static readonly Func<T[], bool> AlwaysFalse = delegate { return false; };
}
public static class ExpressionValueCache<T>
    where T : INumberBase<T>
{
    // Currently, Bilibili's interactive video variables only use integer values between 0~100,
    // but are transmitted as double-precision floating-point strings.
    // Therefore, this library primarily uses double-precision floating-point numbers,
    // but also supports other numeric types.

    private static readonly FrozenDictionary<T, ConstantExpression> _cacheDictionary = Enumerable.Range(0, 101)
        .Select(it => T.CreateTruncating(it))
        .Select(it => new KeyValuePair<T, ConstantExpression>(it, Expression.Constant(it)))
        .ToFrozenDictionary();

    public static ConstantExpression GetConstantOrNew(T key)
    {
        return _cacheDictionary.TryGetValue(key, out ConstantExpression? value) ? value : Expression.Constant(key);
    }
}