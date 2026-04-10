namespace Altemiq.Patterns.Builder.Generators.Examples;

public class WithStringTests
{
    [Test]
    public async Task SetNotNullable()
    {
        await Assert.That(Builder.Examples.WithString.CreateBuilder().WithNotNullable("TEST").Build)
            .ThrowsNothing().And
            .Member(static c => c.NotNullable, static notNullable => notNullable.IsEqualTo("TEST")).And
            .Member(static c => c.Nullable!, static nullable => nullable.IsNull());
    }
}