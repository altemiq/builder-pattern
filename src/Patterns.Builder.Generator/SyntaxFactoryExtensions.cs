// -----------------------------------------------------------------------
// <copyright file="SyntaxFactoryExtensions.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

#pragma warning disable IDE0130, CheckNamespace
namespace Microsoft.CodeAnalysis.CSharp;
#pragma warning restore IDE0130, CheckNamespace

/// <summary>
/// The <see cref="SyntaxFactory"/> extensions.
/// </summary>
internal static class SyntaxFactoryExtensions
{
    extension(SyntaxFactory)
    {
        /// <summary>
        /// Creates a qualified <see cref="Syntax.NameSyntax"/> node.
        /// </summary>
        /// <param name="fullName">The full name.</param>
        /// <returns><see cref="Syntax.NameSyntax"/>.</returns>
        public static Syntax.NameSyntax QualifiedName(string? fullName)
        {
            return GetNames(fullName).ToQualifiedName();

            static IEnumerable<Syntax.IdentifierNameSyntax> GetNames(string? fullName)
            {
                return fullName switch
                {
                    not null => fullName.Split('.').Select(RemoveAttribute).Select(SyntaxFactory.IdentifierName),
                    _ => [],
                };

                static string RemoveAttribute(string name)
                {
                    return name.EndsWith(nameof(Attribute), StringComparison.Ordinal)
                        ? name[..^nameof(Attribute).Length]
                        : name;
                }
            }
        }
    }
}