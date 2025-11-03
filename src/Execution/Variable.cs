using LibBiliInteractiveVideo.API;

namespace LibBiliInteractiveVideo.Execution;

public static class Variable
{
    public static Variable<double> ConvertFromAPI(XSteinEdgeinfoV2.HiddenVar variable) => new()
    {
        Value = variable.Value,
        Id = variable.IdV2 ?? throw new ArgumentNullException($"{nameof(variable)},{nameof(variable.IdV2)}"),
        IsRandom = variable.Type == 2,
        IsShow = variable.Type != 0,
        Name = variable.Name
    };
}
public struct Variable<T>
{
    public T Value { get; set; }
    public string Id { get; set; }
    public bool IsRandom { get; set; }
    public bool IsShow { get; set; }
    public string? Name { get; set; }
}
