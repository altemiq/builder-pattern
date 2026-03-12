// -----------------------------------------------------------------------
// <copyright file="BuilderToGenerate.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.Patterns.Builder.Generator;

/// <summary>
/// The builder to generate.
/// </summary>
/// <param name="BuilderName">The builder name.</param>
/// <param name="FullQualifiedBuilderName">The fully qualified builder name.</param>
/// <param name="ClassName">The class name.</param>
/// <param name="FullyQualifiedClassName">The fully qualified class name.</param>
/// <param name="Namespace">The namespace.</param>
/// <param name="Properties">The properties to generate for.</param>
internal readonly record struct BuilderToGenerate(
    string BuilderName,
    string FullQualifiedBuilderName,
    string ClassName,
    string FullyQualifiedClassName,
    Microsoft.CodeAnalysis.CSharp.SyntaxKind ClassDefinition,
    string Namespace,
    System.Collections.Immutable.IImmutableList<PropertyToGenerate> Properties);