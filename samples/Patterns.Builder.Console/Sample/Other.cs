#pragma warning disable IDE0001, IDE0130

namespace Altemiq.Patterns.Builder.Console.Sample
{
    [Altemiq.Patterns.Builder.GenerateBuilder]
    public partial class Other
    {
        public required MyNamespace.MyCode.Test TestForBuilder { get; init; }

        public System.Collections.Generic.ICollection<MyNamespace.MyCode.Test> Tests { get; } = [];
    }
}
