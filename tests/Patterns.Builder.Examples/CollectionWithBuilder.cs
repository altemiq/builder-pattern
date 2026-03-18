namespace Altemiq.Patterns.Builder.Examples;

[GenerateBuilder]
public partial class CollectionWithBuilder
{
    public System.Collections.Generic.ICollection<Primitive> Values { get; } = [];
}