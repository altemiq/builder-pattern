namespace Altemiq.Patterns.Builder.Generators.Examples;

public class WithTypeConverterTests
{
    [Test]
    public async Task GetDefaultValue()
    {
        await Assert.That(Builder.Examples.WithTypeConverterDefaultValue.CreateBuilder().Build)
            .ThrowsNothing().And
            .Member(static o => o.Size, size => size.IsEqualTo(new(-1, -2)));
    }
}