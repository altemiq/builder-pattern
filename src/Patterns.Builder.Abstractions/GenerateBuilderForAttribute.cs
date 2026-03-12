// -----------------------------------------------------------------------
// <copyright file="GenerateBuilderForAttribute.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.Patterns.Builder;

/// <summary>
/// Indicates that this class should be a builder for the specified class or struct.
/// </summary>
/// <typeparam name="T">The type to generate the builder for.</typeparam>
[AttributeUsage(AttributeTargets.Class)]
public sealed class GenerateBuilderForAttribute<T> : Attribute;