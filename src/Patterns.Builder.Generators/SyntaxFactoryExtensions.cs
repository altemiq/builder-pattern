// -----------------------------------------------------------------------
// <copyright file="SyntaxFactoryExtensions.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.Patterns.Builder.Generators;

/// <summary>
/// The <see cref="SyntaxFactory"/> extensions.
/// </summary>
internal static class SyntaxFactoryExtensions
{
    extension(SyntaxFactory)
    {
        /// <summary>
        /// Creates a qualified <see cref="NameSyntax"/> node.
        /// </summary>
        /// <param name="fullName">The full name.</param>
        /// <returns><see cref="NameSyntax"/>.</returns>
        public static NameSyntax QualifiedName(string fullName) => fullName.Split('.').Select(IdentifierName).ToQualifiedName();
    }
}