namespace Altemiq.Patterns.Builder.Console.Sample
{
    /// <summary>
    /// Axis-aligned bounding box (minx, miny, maxx, maxy) in map/layer coordinate units.
    /// </summary>
    /// <param name="minX">The minimum X-value.</param>
    /// <param name="minY">The minimum Y-value.</param>
    /// <param name="maxX">The maximum X-value.</param>
    /// <param name="maxY">The maximum Y-value.</param>
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public readonly struct BoundingBox(double minX, double minY, double maxX, double maxY)
    {
        /// <summary>
        /// Gets the minimum X value.
        /// </summary>
        public double MinX { get; init; } = minX;

        /// <summary>
        /// Gets the minimum Y value.
        /// </summary>
        public double MinY { get; init; } = minY;

        /// <summary>
        /// Gets the maximum X value.
        /// </summary>
        public double MaxX { get; init; } = maxX;

        /// <summary>
        /// Gets the maximum Y value.
        /// </summary>
        public double MaxY { get; init; } = maxY;

        /// <inheritdoc/>
        public override string ToString() => $"{this.MinX} {this.MinY} {this.MaxX} {this.MaxY}";
    }
}