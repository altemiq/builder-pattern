namespace Altemiq.Patterns.Builder.Console.Sample
{
    [Altemiq.Patterns.Builder.GenerateBuilder]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "RedundantRecordClassKeyword")]
    public record class RecordClass
    {
        public int NotNullable { get; set; }
        public int? Nullable { get; init; }
    }
}