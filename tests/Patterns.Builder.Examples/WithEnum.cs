namespace Altemiq.Patterns.Builder.Examples;

[GenerateBuilder]
public partial class WithEnum
{
    [System.ComponentModel.DefaultValue(FileAccess.Write)]
    public FileAccess FileAccess { get; set; }
}