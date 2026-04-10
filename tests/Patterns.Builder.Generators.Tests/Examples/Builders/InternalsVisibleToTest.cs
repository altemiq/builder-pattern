namespace Altemiq.Patterns.Builder.Generators.Examples.Builders;

public class InternalsVisibleToTest
{
    [Test]
    public async Task SetInternalProperty()
    {
        var dateTime = DateTime.UtcNow;
        await Assert.That(new Builder.Examples.Builders.InternalsVisibleToBuilder().WithInternalProperty(dateTime).Build)
            .ThrowsNothing().And
            .Member(static x => x.InternalProperty, p => p.IsEqualTo(dateTime));
    }
}