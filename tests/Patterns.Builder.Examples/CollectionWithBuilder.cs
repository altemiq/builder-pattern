namespace Altemiq.Patterns.Builder.Examples;

[GenerateBuilder]
public partial class CollectionWithBuilder
{
    public ICollection<Primitive> Values { get; } = [];
}
