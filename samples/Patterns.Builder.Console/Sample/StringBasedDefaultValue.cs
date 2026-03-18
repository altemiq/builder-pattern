namespace Altemiq.Patterns.Builder.Console.Sample
{
    [Altemiq.Patterns.Builder.GenerateBuilder]
    public partial record StringBasedDefaultValue
    {
        [System.ComponentModel.DefaultValue(typeof(System.Drawing.Size), "-1, -2")]
        public System.Drawing.Size Size { get; set; }
    }
}