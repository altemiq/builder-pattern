namespace Altemiq.Patterns.Builder.Examples;

[GenerateBuilder]
public partial class WithTypeConverterDefaultValue
{
    [System.ComponentModel.DefaultValue(typeof(System.Drawing.Size), "-1, -2")]
    public System.Drawing.Size Size { get; set; }
}