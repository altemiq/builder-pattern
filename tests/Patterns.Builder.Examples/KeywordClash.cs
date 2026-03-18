namespace Altemiq.Patterns.Builder.Examples;

[GenerateBuilder]
public partial class KeywordClash
{
    public required Class Class { get; set; }

    public System.Collections.Generic.ICollection<Class> Classes { get; } = [];
}