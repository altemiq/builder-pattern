// -----------------------------------------------------------------------
// <copyright file="BuilderToGenerate.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.Patterns.Builder.Generator;

using System.Collections.Immutable;

/// <summary>
/// The builder to generate.
/// </summary>
/// <param name="BuilderName">The builder name.</param>
/// <param name="ClassName">The class name.</param>
/// <param name="Namespace">The namespace.</param>
/// <param name="FullyQualifiedName">The fully qualified name.</param>
/// <param name="Properties">The properties to generate for.</param>
internal readonly record struct BuilderToGenerate(string BuilderName, string ClassName, string Namespace, string FullyQualifiedName, IImmutableList<PropertyToGenerate> Properties);