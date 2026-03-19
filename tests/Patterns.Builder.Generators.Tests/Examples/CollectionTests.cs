namespace Altemiq.Patterns.Builder.Generators.Examples;

public class CollectionTests
{
    [Test]
    public async Task AddValue()
    {
        await Assert.That(Builder.Examples.Collection.CreateBuilder().AddValue("test").Build())
            .Member(t => t.Values, values => values.IsEquivalentTo(["test"]));
    }

    [Test]
    public async Task AddValueViaConstructor()
    {
        await Assert.That(Builder.Examples.Collection.CreateBuilder().AddValue(['t', 'e', 's', 't']).Build())
            .Member(t => t.Values, values => values.IsEquivalentTo(["test"]));
    }
}