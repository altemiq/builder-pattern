// -----------------------------------------------------------------------
// <copyright file="IBuilder{T}.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.Patterns.Builder;

/// <summary>
/// Interface for the builder.
/// </summary>
/// <typeparam name="T">The type to build.</typeparam>
public interface IBuilder<out T>
{
    /// <summary>
    /// Builds an instance of <typeparamref name="T"/>.
    /// </summary>
    /// <returns>The instance of <typeparamref name="T"/>.</returns>
    T Build();
}