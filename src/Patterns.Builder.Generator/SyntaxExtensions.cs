// -----------------------------------------------------------------------
// <copyright file="SyntaxExtensions.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

#pragma warning disable IDE0130, CheckNamespace
namespace Microsoft.CodeAnalysis.CSharp;
#pragma warning restore IDE0130, CheckNamespace

#pragma warning disable RCS1263, SA1101

/// <summary>
/// The syntax extensions.
/// </summary>
internal static class SyntaxExtensions
{
    /// <content>
    /// The <see cref="Type"/> extensions.
    /// </content>
    /// <param name="type">The type.</param>
    extension(Type type)
    {
        /// <summary>
        /// Converts the type to the <see cref="Syntax.TypeSyntax"/>.
        /// </summary>
        /// <param name="parameters">The type parameters.</param>
        /// <returns>The type syntax.</returns>
        /// <exception cref="ArgumentOutOfRangeException">The parameters are the wrong length.</exception>
        public Syntax.NameSyntax ToTypeSyntax(IEnumerable<Syntax.TypeSyntax> parameters)
        {
            if (!type.IsGenericTypeDefinition)
            {
                return type.FullName is { } fullName
                    ? SyntaxFactory.QualifiedName(fullName)
                    : throw new InvalidOperationException();
            }

            var index = type.Name.IndexOf('`');
            var name = type.Name[..index];
            var count = int.Parse(type.Name[(index + 1)..], System.Globalization.CultureInfo.InvariantCulture);

            var parameterList = SyntaxFactory.SeparatedList(parameters);
            if (count != parameterList.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(parameters));
            }

            var genericName = SyntaxFactory.GenericName(SyntaxFactory.Identifier(name), SyntaxFactory.TypeArgumentList(parameterList));
            return type is { Namespace: { } n }
                ? SyntaxFactory.QualifiedName(SyntaxFactory.QualifiedName(n), genericName)
                : genericName;
        }
    }

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