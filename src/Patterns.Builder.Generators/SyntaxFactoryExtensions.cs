// -----------------------------------------------------------------------
// <copyright file="SyntaxFactoryExtensions.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

#pragma warning disable IDE0130, CheckNamespace
namespace Altemiq.Patterns.Builder.Generators;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

#pragma warning restore IDE0130, CheckNamespace

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
        public static Microsoft.CodeAnalysis.CSharp.Syntax.NameSyntax QualifiedName(string fullName) => fullName.Split('.').Select(SyntaxFactory.IdentifierName).ToQualifiedName();
    }
}