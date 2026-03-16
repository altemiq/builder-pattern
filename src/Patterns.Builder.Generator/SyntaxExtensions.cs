// -----------------------------------------------------------------------
// <copyright file="SyntaxExtensions.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.Patterns.Builder.Generator;

/// <summary>
/// The syntax extensions.
/// </summary>
internal static class SyntaxExtensions
{
    /// <summary>
    /// Converts the simple names to a qualified name.
    /// </summary>
    /// <param name="names">The names to qualify.</param>
    /// <returns>The qualified name.</returns>
    /// <exception cref="InvalidOperationException">Could not generate the name.</exception>
    public static Microsoft.CodeAnalysis.CSharp.Syntax.NameSyntax ToQualifiedName(this IEnumerable<Microsoft.CodeAnalysis.CSharp.Syntax.SimpleNameSyntax> names)
    {
        var enumerator = names.GetEnumerator();
        _ = enumerator.MoveNext();

        Microsoft.CodeAnalysis.CSharp.Syntax.NameSyntax? name = enumerator.Current;
        while (enumerator.MoveNext() && name is not null && enumerator.Current is not null)
        {
            name = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.QualifiedName(name, enumerator.Current);
        }

        enumerator.Dispose();
        return name ?? throw new InvalidOperationException();
    }
}