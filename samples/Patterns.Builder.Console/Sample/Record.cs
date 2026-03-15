namespace Altemiq.Patterns.Builder.Console.Sample
{
    [Altemiq.Patterns.Builder.GenerateBuilder]
    public record Record
    {
        public int NotNullable { get; set; }
        public int? Nullable { get; init; }
    }
}
