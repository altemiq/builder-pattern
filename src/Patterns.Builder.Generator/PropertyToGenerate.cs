// -----------------------------------------------------------------------
// <copyright file="PropertyToGenerate.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.Patterns.Builder.Generator;

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
    /// Creates a new instance of the <see cref="PropertyToGenerate"/> struct.
    /// </summary>
    /// <param name="propertySymbol">The property symbol.</param>
    /// <param name="collectionTypeSymbol">The collection symbol.</param>
    /// <param name="dictionaryTypeSymbol">The dictionary symbol.</param>
    /// <returns>The property to generate.</returns>
    public static PropertyToGenerate Create(IPropertySymbol propertySymbol, ITypeSymbol? collectionTypeSymbol, ITypeSymbol? dictionaryTypeSymbol)
    {
        var name = propertySymbol.Name;
        var fieldName = name.Camelize();
        var type = propertySymbol.Type;
        var typeSyntax = type.ToType();
        var primitive = type.IsPrimitiveOrNullablePrimitive;
        var readOnly = propertySymbol.IsReadOnly;
        var collection = type.IsCollection(collectionTypeSymbol);
        var dictionary = type.IsDictionary(dictionaryTypeSymbol);
        var accessibility = GetAccessibility(propertySymbol, readOnly, collection);

        return new(name, fieldName, typeSyntax, primitive, accessibility, readOnly, collection, dictionary);

        static Microsoft.CodeAnalysis.CSharp.SyntaxKind GetAccessibility(IPropertySymbol propertySymbol, bool readOnly, bool collection)
        {
            if (readOnly && collection && propertySymbol.GetMethod is { DeclaredAccessibility: var getDeclaredAccessibility })
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

    /// <inheritdoc/>
    public readonly bool Equals(PropertyToGenerate other) => StringComparer.Ordinal.Equals(this.Name, other.Name)
        && StringComparer.Ordinal.Equals(this.FieldName, other.FieldName)
        && StringComparer.Ordinal.Equals(this.Type.ToFullString(), other.Type.ToFullString())
        && this.Primitive == other.Primitive
        && this.Accessibility == other.Accessibility
        && this.ReadOnly == other.ReadOnly
        && this.Collection == other.Collection
        && this.Dictionary == other.Dictionary;

    /// <inheritdoc/>
    public readonly override int GetHashCode()
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

    /// <summary>
    /// Tries to get the builder for this property.
    /// </summary>
    /// <param name="builders">The potential builders.</param>
    /// <param name="builder">The builder if found.</param>
    /// <returns><see langword="true"/> if a builder is found; otherwise <see langword="false"/>.</returns>
    public readonly bool TryGetBuilder(IEnumerable<BuilderToGenerate> builders, out BuilderToGenerate builder)
    {
        var type = this.Type;

        if (type is Microsoft.CodeAnalysis.CSharp.Syntax.QualifiedNameSyntax { Right: Microsoft.CodeAnalysis.CSharp.Syntax.GenericNameSyntax { TypeArgumentList.Arguments: { } arguments } })
        {
            // we have arguments
            if (arguments.Count is not 1)
            {
                builder = default;
                return false;
            }

            type = arguments[0];
        }

        var typeName = type.ToFullString();
        builder = builders.FirstOrDefault(potentialBuilder => StringComparer.Ordinal.Equals(potentialBuilder.FullyQualifiedClassName, typeName));
        return builder.ClassName is not null;
    }
}