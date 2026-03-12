// -----------------------------------------------------------------------
// <copyright file="TypeNames.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.Patterns.Builder.Generator;

/// <summary>
/// The type names.
/// </summary>
internal static class TypeNames
{
    public const string GenerateBuilderForAttributeShortName = "GenerateBuilderFor";

    public const string GenerateBuilderForAttributeLongName = $"{GenerateBuilderForAttributeShortName}Attribute";

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