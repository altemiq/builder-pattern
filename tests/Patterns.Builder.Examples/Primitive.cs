namespace Altemiq.Patterns.Builder.Examples;

[GenerateBuilder]
public partial class Primitive
{
    public int NotNullable { get; set; }
    public int? Nullable { get; init; }
}
