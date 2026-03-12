namespace Altemiq.Patterns.Builder.Generator.Examples;

public class WithStringTests
{
    [Test]
    public async Task SetNotNullable()
    {
        await Assert.That(Builder.Examples.WithString.CreateBuilder().WithNotNullable("TEST").Build())
            .Member(c => c.NotNullable, notNullable => notNullable.IsEqualTo("TEST")).And
            .Member(c => c.Nullable!, nullable => nullable.IsNull());
    }
}
