namespace Altemiq.Patterns.Builder.Generators.Examples;

public class CollectionTests
{
    [Test]
    public async Task AddValue()
    {
        await Assert.That(Builder.Examples.Collection.CreateBuilder().AddValue("test").Build)
            .ThrowsNothing().And
            .Member(static t => t.Values, static values => values.IsEquivalentTo(["test"]));
    }

    [Test]
    public async Task AddValueViaConstructor()
    {
        await Assert.That(Builder.Examples.Collection.CreateBuilder().AddValue(['t', 'e', 's', 't']).Build)
            .ThrowsNothing().And
            .Member(static t => t.Values, static values => values.IsEquivalentTo(["test"]));
    }
}