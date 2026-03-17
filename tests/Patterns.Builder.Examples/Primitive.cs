namespace Altemiq.Patterns.Builder.Examples;

[GenerateBuilder]
public partial record class Primitive
{
    public int NotNullable { get; set; }
    public int? Nullable { get; init; }
}