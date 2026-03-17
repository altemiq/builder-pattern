namespace Altemiq.Patterns.Builder.Examples;

[GenerateBuilder]
public partial class Collection
{
    public ICollection<string> Values { get; } = [];
}