namespace Altemiq.Patterns.Builder.Generators.Examples;

public class DictionaryTests
{
    [Test]
    public async Task AddKeyValue()
    {
        var builder = Builder.Examples.Dictionary.CreateBuilder();
        var dictionary = await Assert.That(builder.AddValue(1, "test").Build()).IsNotNull();
        await Assert.That(dictionary.Values).ContainsKeyWithValue(1, "test");
    }
}