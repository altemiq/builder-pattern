namespace Patterns.Builder.Console.Sample
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    [Altemiq.Patterns.Builder.GenerateBuilder]
    internal partial class KeywordClash
    {
        public required Class Class { get; set; }
        public System.Collections.Generic.ICollection<Class> Classes { get; } = [];
    }

    internal sealed record Class;
}
