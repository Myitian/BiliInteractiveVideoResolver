namespace LibBiliInteractiveVideo.Execution;

public struct ExtraInfo
{
    public int ValueIndex { get; set; }
    public string Id { get; set; }
    public bool IsRandom { get; set; }
    public bool IsShow { get; set; }
    public string? Name { get; set; }

    public override readonly string ToString()
        => $"<{ValueIndex}:{Id}:{Name}>";
}