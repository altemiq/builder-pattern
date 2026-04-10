namespace Altemiq.Patterns.Builder.Generators.Examples;

public class WithEnumTests
{
    [Test]
    public async Task UseDefault()
    {
        await Assert.That(Builder.Examples.WithEnum.CreateBuilder().Build)
            .ThrowsNothing().And
            .Member(static x => x.FileAccess, static fileAccess => fileAccess.IsEqualTo(FileAccess.Write));
    }

    [Test]
    public async Task SetEnum()
    {
        await Assert.That(Builder.Examples.WithEnum.CreateBuilder().WithFileAccess(FileAccess.ReadWrite).Build)
            .ThrowsNothing().And
            .Member(static x => x.FileAccess, static fileAccess => fileAccess.IsEqualTo(FileAccess.ReadWrite));
    }
}