namespace Altemiq.Patterns.Builder.Generators.Examples;

public class PrimitiveTests
{
    [Test]
    public async Task SetValue()
    {
        const int value = 10;
        await Assert.That(Builder.Examples.Primitive.CreateBuilder().WithNotNullable(value).Build)
            .ThrowsNothing().And
            .Member(static x => x.NotNullable, static v => v.IsEqualTo(value)).And
            .Member(static x => x.Nullable, static v => v.IsNull());
    }
}