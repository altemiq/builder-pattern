namespace Altemiq.Patterns.Builder.Examples;

[GenerateBuilder]
public partial class WithString
{
    public required string NotNullable { get; set; }
    public string? Nullable { get; set; }
}
