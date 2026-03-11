// -----------------------------------------------------------------------
// <copyright file="GenerateBuilderAttribute.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.Patterns.Builder;

/// <summary>
/// Indicates that a builder should be generated for this class or struct.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public sealed class GenerateBuilderAttribute : Attribute;