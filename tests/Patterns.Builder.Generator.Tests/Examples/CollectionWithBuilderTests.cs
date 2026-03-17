namespace Altemiq.Patterns.Builder.Generator.Examples;

public class CollectionWithBuilderTests
{
    [Test]
    public async Task AddValue()
    {
        var builder = Builder.Examples.CollectionWithBuilder.CreateBuilder();
        await Assert.That(builder.AddValue(builder => builder.WithNotNullable(1)).Build())
            .Member(t => t.Values, values => values.IsEquivalentTo([new Builder.Examples.Primitive { NotNullable = 1 }]));
    }
}