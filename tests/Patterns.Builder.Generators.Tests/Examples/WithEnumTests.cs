namespace Altemiq.Patterns.Builder.Generators.Examples;

public class WithEnumTests
{
    [Test]
    public async Task UseDefault()
    {
        await Assert.That(Builder.Examples.WithEnum.CreateBuilder().Build)
            .ThrowsNothing().And
            .Member(x => x.FileAccess, fileAccess => fileAccess.IsEqualTo(FileAccess.Write));
    }

    [Test]
    public async Task SetEnum()
    {
        ;
        await Assert.That(Builder.Examples.WithEnum.CreateBuilder().WithFileAccess(FileAccess.ReadWrite).Build)
            .ThrowsNothing().And
            .Member(x => x.FileAccess, fileAccess => fileAccess.IsEqualTo(FileAccess.ReadWrite));
    }
}