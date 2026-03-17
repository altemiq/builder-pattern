namespace Altemiq.Patterns.Builder.Console.Sample
{
    [Altemiq.Patterns.Builder.GenerateBuilder]
    public record struct RecordStruct
    {
        public int NotNullable { get; set; }
        public int? Nullable { get; init; }
    }
}