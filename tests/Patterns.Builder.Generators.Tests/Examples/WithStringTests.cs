namespace Altemiq.Patterns.Builder.Generators.Examples;

public class WithStringTests
{
    [Test]
    public async Task SetNotNullable()
    {
        await Assert.That(Builder.Examples.WithString.CreateBuilder().WithNotNullable("TEST").Build)
            .ThrowsNothing().And
            .Member(c => c.NotNullable, notNullable => notNullable.IsEqualTo("TEST")).And
            .Member(c => c.Nullable!, nullable => nullable.IsNull());
    }
}