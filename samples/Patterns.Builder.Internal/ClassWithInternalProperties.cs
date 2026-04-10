namespace Altemiq.Patterns.Builder.Internal
{
    using System;
    using System.Collections.Generic;

    public class ClassWithInternalProperties
    {
        internal DateTime InternalProperty { get; set; }

        public DateTime InternalGetProperty { internal get; set; }

        public DateTime InternalSetProperty { get; internal set; }

        internal ICollection<DateTime> InternalCollectionProperty { get; } = new List<DateTime>();
    }
}