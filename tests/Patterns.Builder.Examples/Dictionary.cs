namespace Altemiq.Patterns.Builder.Examples;

[GenerateBuilder]
public partial class Dictionary
{
    public IDictionary<int, string> Values { get; } = new Dictionary<int, string>();
}
