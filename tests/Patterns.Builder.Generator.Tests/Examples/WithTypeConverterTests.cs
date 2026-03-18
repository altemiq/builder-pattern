namespace Altemiq.Patterns.Builder.Generator.Examples;

public class WithTypeConverterTests
{
    [Test]
    public async Task GetDefaultValue()
    {
        var builder = Builder.Examples.WithTypeConverterDefaultValue.CreateBuilder();
        await Assert.That(builder.Build()).Member(static o => o.Size, size => size.IsEqualTo(new(-1, -2)));
    }
}