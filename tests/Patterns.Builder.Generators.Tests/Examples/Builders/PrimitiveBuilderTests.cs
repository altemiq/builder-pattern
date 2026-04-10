namespace Altemiq.Patterns.Builder.Generators.Examples.Builders;

public class PrimitiveBuilderTests
{
    [Test]
    public async Task SetValue()
    {
        const int value = 10;
        await Assert.That(new Builder.Examples.Builders.PrimitiveBuilder().WithNotNullable(value).Build)
            .ThrowsNothing().And
            .Member(static x => x.NotNullable, static v => v.IsEqualTo(value)).And
            .Member(static x => x.Nullable, static v => v.IsNull());
    }
}