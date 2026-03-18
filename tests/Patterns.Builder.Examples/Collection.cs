namespace Altemiq.Patterns.Builder.Examples;

[GenerateBuilder]
public partial class Collection
{
    public System.Collections.Generic.ICollection<string> Values { get; } = [];
}