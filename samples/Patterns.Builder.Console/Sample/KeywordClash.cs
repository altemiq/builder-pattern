namespace Altemiq.Patterns.Builder.Console.Sample
{
    [Altemiq.Patterns.Builder.GenerateBuilder]
    internal partial class KeywordClash
    {
        public required Class Class { get; set; }
        public System.Collections.Generic.ICollection<Class> Classes { get; } = [];
    }

    internal sealed record Class;
}