namespace Altemiq.Patterns.Builder.Generators.Examples;

public class WithBuilderTests
{
    [Test]
    public async Task Direct()
    {
        var builder = Builder.Examples.WithBuilder.CreateBuilder();
        builder.WithPrimitiveViaBuilder(new Builder.Examples.Primitive { NotNullable = 1 });
        await Assert.That(builder.Build())
        .Member(c => c.PrimitiveViaBuilder, primitive => primitive.IsEqualTo(new Builder.Examples.Primitive { NotNullable = 1 }));
    }

    [Test]
    public async Task ViaFactory()
    {
        var builder = Builder.Examples.WithBuilder.CreateBuilder();
        builder.WithPrimitiveViaBuilder(() => new Builder.Examples.Primitive { NotNullable = 1 });
        await Assert.That(builder.Build())
        .Member(c => c.PrimitiveViaBuilder, primitive => primitive.IsEqualTo(new Builder.Examples.Primitive { NotNullable = 1 }));
    }

    [Test]
    public async Task ViaBuilder()
    {
        var builder = Builder.Examples.WithBuilder.CreateBuilder();
        builder.WithPrimitiveViaBuilder(builder => builder.WithNotNullable(1));
        await Assert.That(builder.Build())
        .Member(c => c.PrimitiveViaBuilder, primitive => primitive.IsEqualTo(new Builder.Examples.Primitive { NotNullable = 1 }));
    }
}