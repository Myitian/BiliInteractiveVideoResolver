namespace LibBiliInteractiveVideo.Execution.Compilation;

public enum ConditionOperation
{
    /// <summary>==</summary>
    EQ,
    /// <summary>&lt;=</summary>
    LE,
    /// <summary>&lt;</summary>
    LT,
    /// <summary>&gt;=</summary>
    GE,
    /// <summary>&gt;</summary>
    /// <remarks>all other state will be treated as <see cref="GT"/></remarks>
    GT
}
