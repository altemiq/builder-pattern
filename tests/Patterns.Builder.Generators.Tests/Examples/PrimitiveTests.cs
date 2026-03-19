namespace Altemiq.Patterns.Builder.Generators.Examples;

public class PrimitiveTests
{
    [Test]
    public async Task SetValue()
    {
        const int Value = 10;
        await Assert.That(Builder.Examples.Primitive.CreateBuilder().WithNotNullable(Value).Build)
            .ThrowsNothing().And
            .Member(x => x.NotNullable, value => value.IsEqualTo(Value)).And
            .Member(x => x.Nullable, value => value.IsNull());
    }
}