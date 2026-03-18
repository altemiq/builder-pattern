namespace Altemiq.Patterns.Builder.Examples;

[GenerateBuilder]
public partial class Dictionary
{
    public System.Collections.Generic.IDictionary<int, string> Values { get; } = new System.Collections.Generic.Dictionary<int, string>();
}