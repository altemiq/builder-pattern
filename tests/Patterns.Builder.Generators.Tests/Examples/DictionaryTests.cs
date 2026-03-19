namespace Altemiq.Patterns.Builder.Generators.Examples;

public class DictionaryTests
{
    [Test]
    public async Task AddKeyValue()
    {
        var dictionary = await Assert.That(Builder.Examples.Dictionary.CreateBuilder().AddValue(1, "test").Build)
            .ThrowsNothing().And
            .IsNotNull();
        await Assert.That(dictionary.Values).ContainsKeyWithValue(1, "test");
    }
}