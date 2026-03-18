namespace Altemiq.Patterns.Builder.Examples;

[GenerateBuilder]
public partial struct WithEnum
{
    [System.ComponentModel.DefaultValue(System.IO.FileAccess.Write)]
    public System.IO.FileAccess FileAccess { get; set; }
}