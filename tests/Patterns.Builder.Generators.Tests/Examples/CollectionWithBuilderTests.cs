namespace Altemiq.Patterns.Builder.Generators.Examples;

public class CollectionWithBuilderTests
{
    [Test]
    public async Task AddValue()
    {
        var builder = Builder.Examples.CollectionWithBuilder.CreateBuilder();
        await Assert.That(builder.AddValue(builder => builder.WithNotNullable(1)).Build)
            .ThrowsNothing().And
            .Member(t => t.Values, values => values.IsEquivalentTo([new Builder.Examples.Primitive { NotNullable = 1 }]));
    }
}