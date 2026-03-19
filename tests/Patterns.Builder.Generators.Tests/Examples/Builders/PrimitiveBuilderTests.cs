namespace Altemiq.Patterns.Builder.Generators.Examples.Builders;

public class PrimitiveBuilderTests
{
    [Test]
    public async Task SetValue()
    {
        const int Value = 10;
        var builder = new Builder.Examples.Builders.PrimitiveBuilder();
        await Assert.That(builder).IsNotNull();

        await Assert.That(builder.WithNotNullable(Value)).IsNotNull();
        await Assert.That(builder.Build)
            .ThrowsNothing().And
            .Member(x => x.NotNullable, value => value.IsEqualTo(Value)).And
            .Member(x => x.Nullable, value => value.IsNull());
    }
}