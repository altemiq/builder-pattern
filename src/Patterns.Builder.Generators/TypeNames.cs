// -----------------------------------------------------------------------
// <copyright file="TypeNames.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.Patterns.Builder.Generators;

/// <summary>
/// The type names.
/// </summary>
internal static class TypeNames
{
    /// <summary>
    /// The GenerateBuilderFor short name.
    /// </summary>
    public const string GenerateBuilderForAttributeShortName = "GenerateBuilderFor";

    /// <summary>
    /// The GenerateBuilderFor long name.
    /// </summary>
    public const string GenerateBuilderForAttributeLongName = $"{GenerateBuilderForAttributeShortName}Attribute";

    /// <summary>
    /// The DefaultValue short name.
    /// </summary>
    public const string DefaultValueAttributeShortName = "DefaultValue";

    /// <summary>
    /// The DefaultValue long name.
    /// </summary>
    public const string DefaultValueAttributeLongName = $"{DefaultValueAttributeShortName}Attribute";

    /// <summary>
    /// The markers.
    /// </summary>
    public static class Markers
    {
        /// <summary>
        /// The GenerateBuilder marker attribute.
        /// </summary>
        public const string GenerateBuilderAttribute = "Altemiq.Patterns.Builder.GenerateBuilderAttribute";

        /// <summary>
        /// The GenerateBuilderFor marker attribute.
        /// </summary>
        public const string GenerateBuilderForAttribute = $"Altemiq.Patterns.Builder.{GenerateBuilderForAttributeLongName}`1";
    }
}