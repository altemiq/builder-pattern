namespace Altemiq.Patterns.Builder.Generators.Examples;

public class WithEnumTests
{
    [Test]
    public async Task UseDefault()
    {
        var builder = Builder.Examples.WithEnum.CreateBuilder();
        await Assert.That(builder.Build()).Member(x => x.FileAccess, fileAccess => fileAccess.IsEqualTo(FileAccess.Write));
    }

    [Test]
    public async Task SetEnum()
    {
        var builder = Builder.Examples.WithEnum.CreateBuilder();
        builder.WithFileAccess(FileAccess.ReadWrite);
        await Assert.That(builder.Build()).Member(x => x.FileAccess, fileAccess => fileAccess.IsEqualTo(FileAccess.ReadWrite));
    }
}