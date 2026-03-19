namespace Altemiq.Patterns.Builder.Generators.Examples.Builders;

public class InternalsVisibleToTest
{
    [Test]
    public async Task SetInternalProperty()
    {
        var builder = new Builder.Examples.Builders.InternalsVisibleToBuilder();
        var dateTime = DateTime.UtcNow;
        await Assert.That(builder.WithInternalProperty(dateTime).Build)
            .ThrowsNothing().And
            .Member(x => x.InternalProperty, p => p.IsEqualTo(dateTime));
    }
}