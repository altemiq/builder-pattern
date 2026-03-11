// -----------------------------------------------------------------------
// <copyright file="PropertyToGenerate.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.Patterns.Builder.Generator;

using System.ComponentModel.DataAnnotations;
using Humanizer;

/// <summary>
/// The property to generate.
/// </summary>
/// <param name="Name">The name.</param>
/// <param name="FieldName">The field name.</param>
/// <param name="Type">The type.</param>
/// <param name="Primitive">Whether this is a primitive.</param>
/// <param name="Accessibility">The accessibility.</param>
/// <param name="Collection">Whether this is a collection.</param>
/// <param name="Dictionary">Whether this is a dictionary.</param>
internal readonly record struct PropertyToGenerate(
    string Name,
    string FieldName,
    Microsoft.CodeAnalysis.CSharp.Syntax.TypeSyntax Type,
    bool Primitive,
    Microsoft.CodeAnalysis.CSharp.SyntaxKind Accessibility,
    bool ReadOnly,
    bool Collection,
    bool Dictionary)
{
    /// <summary>
    /// Initialises a new instance of the <see cref="PropertyToGenerate"/> struct.
    /// </summary>
    /// <param name="propertySymbol">The property symbol.</param>
    /// <param name="collectionTypeSymbol">The collection symbol.</param>
    /// <param name="dictionaryTypeSymbol">The dictionary symbol.</param>
    public PropertyToGenerate(IPropertySymbol propertySymbol, ITypeSymbol? collectionTypeSymbol, ITypeSymbol? dictionaryTypeSymbol)
        : this(
              propertySymbol.Name,
              propertySymbol.Name.Camelize(),
              propertySymbol.Type.ToType(),
              propertySymbol.Type.IsPrimitiveOrNullablePrimitive,
              GetAccessibility(propertySymbol, collectionTypeSymbol),
              propertySymbol.IsReadOnly,
              propertySymbol.Type.IsCollection(collectionTypeSymbol),
              propertySymbol.Type.IsDictionary(dictionaryTypeSymbol))
    {
    }

    // Optional: implement IEquatable<Point> for performance
    public bool Equals(PropertyToGenerate other)
    {
        return StringComparer.Ordinal.Equals(this.Name, other.Name)
            && StringComparer.Ordinal.Equals(this.FieldName, other.FieldName)
            && StringComparer.Ordinal.Equals(this.Type.ToFullString(), other.Type.ToFullString())
            && this.Primitive == other.Primitive
            && this.Accessibility == other.Accessibility
            && this.ReadOnly == other.ReadOnly
            && this.Collection == other.Collection
            && this.Dictionary == other.Dictionary;
    }

    public override int GetHashCode()
    {
        var hash = 0;
        hash += StringComparer.Ordinal.GetHashCode(this.Name);
        hash += 19 + StringComparer.Ordinal.GetHashCode(this.Name);
        hash += 38 + StringComparer.Ordinal.GetHashCode(this.Type.ToFullString());
        hash += 57 + this.Primitive.GetHashCode();
        hash += 76 + this.Accessibility.GetHashCode();
        hash += 95 + this.ReadOnly.GetHashCode();
        hash += 114 + this.Collection.GetHashCode();
        hash += 133 + this.Dictionary.GetHashCode();

        return hash;
    }

    private static bool TypeSyntaxEquals(Microsoft.CodeAnalysis.CSharp.Syntax.TypeSyntax first, Microsoft.CodeAnalysis.CSharp.Syntax.TypeSyntax second)
    {
        // only do the parts we are about
        if (first == second)
        {
            return true;
        }

        return false;
    }

    private static int TypeSyntaxHashCode(Microsoft.CodeAnalysis.CSharp.Syntax.TypeSyntax first)
    {
        var hashCode = 0;
        return hashCode;
    }

    private static Microsoft.CodeAnalysis.CSharp.SyntaxKind GetAccessibility(IPropertySymbol propertySymbol, ITypeSymbol? collectionTypeSymbol)
    {
        if (propertySymbol.IsReadOnly
            && propertySymbol.Type.IsCollection(collectionTypeSymbol)
            && propertySymbol.GetMethod is { DeclaredAccessibility: var getDeclaredAccessibility })
        {
            return getDeclaredAccessibility switch
            {
                Microsoft.CodeAnalysis.Accessibility.Internal => Microsoft.CodeAnalysis.CSharp.SyntaxKind.InternalKeyword,
                Microsoft.CodeAnalysis.Accessibility.Public => Microsoft.CodeAnalysis.CSharp.SyntaxKind.PublicKeyword,
                _ => Microsoft.CodeAnalysis.CSharp.SyntaxKind.PrivateKeyword,
            };
        }

        if (propertySymbol.SetMethod is { DeclaredAccessibility: var setDeclaredAccessibility })
        {
            return setDeclaredAccessibility switch
            {
                Microsoft.CodeAnalysis.Accessibility.Internal => Microsoft.CodeAnalysis.CSharp.SyntaxKind.InternalKeyword,
                Microsoft.CodeAnalysis.Accessibility.Public => Microsoft.CodeAnalysis.CSharp.SyntaxKind.PublicKeyword,
                _ => Microsoft.CodeAnalysis.CSharp.SyntaxKind.PrivateKeyword,
            };
        }

        return Microsoft.CodeAnalysis.CSharp.SyntaxKind.PrivateKeyword;
    }
}