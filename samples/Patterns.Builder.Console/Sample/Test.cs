namespace MyNamespace.MyCode
{
    [Altemiq.Patterns.Builder.GenerateBuilder]
    public partial class Test
    {
        internal string? Comments { get; set; }
        public System.Collections.Generic.Dictionary<int, string> Additional { get; } = [];
        public System.Collections.Generic.ICollection<int> Values { get; } = [];
        [System.ComponentModel.DefaultValue(5)]
        public int Quality { get; set; } = 5;
        public int? Rating { get; set; }
        public System.DateTime Ordered { get; set; }
    }
}