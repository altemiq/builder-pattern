// -----------------------------------------------------------------------
// <copyright file="SyntaxExtensions.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

#pragma warning disable IDE0130, CheckNamespace
namespace Microsoft.CodeAnalysis.CSharp;
#pragma warning restore IDE0130, CheckNamespace

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
    public static Syntax.NameSyntax ToQualifiedName(this IEnumerable<Syntax.SimpleNameSyntax> names)
    {
        var enumerator = names.GetEnumerator();
        _ = enumerator.MoveNext();

        Syntax.NameSyntax? name = enumerator.Current;
        while (enumerator.MoveNext() && name is not null && enumerator.Current is not null)
        {
            name = SyntaxFactory.QualifiedName(name, enumerator.Current);
        }

        enumerator.Dispose();
        return name ?? throw new InvalidOperationException();
    }
}