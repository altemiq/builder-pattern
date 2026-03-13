// -----------------------------------------------------------------------
// <copyright file="PropertyMetadata.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.Patterns.Builder.Generator;

/// <summary>
/// The property metadata.
/// </summary>
[Flags]
internal enum PropertyMetadata
{
    /// <summary>
    /// None.
    /// </summary>
    None = 0,

    /// <summary>
    /// Primitive.
    /// </summary>
    Primitive = 1 << 0,

    /// <summary>
    /// Read only.
    /// </summary>
    ReadOnly = 1 << 1,

    /// <summary>
    /// Collection.
    /// </summary>
    Collection = 1 << 2,

    /// <summary>
    /// Dictionary.
    /// </summary>
    Dictionary = 1 << 3,

    /// <summary>
    /// Nullable.
    /// </summary>
    Nullable = 1 << 4,
}