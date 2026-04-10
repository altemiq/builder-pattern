namespace Altemiq.Patterns.Builder.Generators.Examples;

public class WithBuilderTests
{
    [Test]
    public async Task Direct()
    {
        await Assert.That(Builder.Examples.WithBuilder.CreateBuilder().WithPrimitive(new Builder.Examples.Primitive { NotNullable = 1 }).Build)
            .ThrowsNothing().And
            .Member(c => c.Primitive, primitive => primitive.IsEqualTo(new() { NotNullable = 1 }));
    }

    [Test]
    public async Task ViaFactory()
    {
        await Assert.That(Builder.Examples.WithBuilder.CreateBuilder().WithPrimitive(() => new() { NotNullable = 1 }).Build)
            .ThrowsNothing().And
            .Member(c => c.Primitive, primitive => primitive.IsEqualTo(new() { NotNullable = 1 }));
    }

    [Test]
    public async Task ViaBuilder()
    {
        await Assert.That(Builder.Examples.WithBuilder.CreateBuilder().WithPrimitive(builder => builder.WithNotNullable(1)).Build)
            .ThrowsNothing().And
            .Member(c => c.Primitive, primitive => primitive.IsEqualTo(new() { NotNullable = 1 }));
    }
}