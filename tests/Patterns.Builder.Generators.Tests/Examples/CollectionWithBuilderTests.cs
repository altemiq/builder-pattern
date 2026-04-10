namespace Altemiq.Patterns.Builder.Generators.Examples;

public class CollectionWithBuilderTests
{
    [Test]
    public async Task AddValue()
    {
        await Assert.That(Builder.Examples.CollectionWithBuilder.CreateBuilder().AddValue(builder => builder.WithNotNullable(1)).Build)
            .ThrowsNothing().And
            .Member(static t => t.Values, static values => values.IsEquivalentTo([new Builder.Examples.Primitive { NotNullable = 1 }]));
    }
}