namespace Altemiq.Patterns.Builder.Generator.Examples;

public class CollectionTests
{
    [Test]
    public async Task AddValue()
    {
        var builder = Builder.Examples.Collection.CreateBuilder();
        await Assert.That(builder.AddValue("test").Build())
            .Member(t => t.Values, values => values.IsEquivalentTo(["test"]));
    }
}