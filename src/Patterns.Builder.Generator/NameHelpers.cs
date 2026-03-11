// -----------------------------------------------------------------------
// <copyright file="NameHelpers.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.Patterns.Builder.Generator;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

/// <summary>
/// The helpers.
/// </summary>
internal static class NameHelpers
{
    /// <summary>
    /// Gets the qualified name.
    /// </summary>
    /// <param name="fullName">The full name.</param>
    /// <returns>The qualified name.</returns>
    /// <exception cref="InvalidOperationException"><paramref name="fullName"/> is invalid.</exception>
    public static NameSyntax GetQualifiedName(string fullName) => GetQualifiedName(GetNames(fullName));

    /// <summary>
    /// Gets the qualified name.
    /// </summary>
    /// <param name="names">The names.</param>
    /// <returns>The qualified name.</returns>
    /// <exception cref="InvalidOperationException"><paramref name="fullName"/> is invalid.</exception>
    public static NameSyntax GetQualifiedName(IEnumerable<IdentifierNameSyntax> names)
    {
        var enumerator = names.GetEnumerator();
        _ = enumerator.MoveNext();

        NameSyntax name = enumerator.Current;
        while (enumerator.MoveNext() && name is not null && enumerator.Current is not null)
        {
            name = SyntaxFactory.QualifiedName(name, enumerator.Current);
        }

        enumerator.Dispose();
        return name ?? throw new InvalidOperationException();
    }

    /// <summary>
    /// Gets the names from the full name.
    /// </summary>
    /// <param name="fullName">The full name.</param>
    /// <returns>The names.</returns>
    public static IEnumerable<IdentifierNameSyntax> GetNames(string fullName)
    {
        return fullName.Split('.').Select(RemoveAttribute).Select(SyntaxFactory.IdentifierName);

        static string RemoveAttribute(string name)
        {
            return name.EndsWith(nameof(Attribute), StringComparison.Ordinal)
                ? name[..^nameof(Attribute).Length]
                : name;
        }
    }
}