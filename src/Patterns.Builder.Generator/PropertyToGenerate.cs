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
/// <param name="Metadata">The property metadata.</param>
/// <param name="Accessibility">The accessibility.</param>
/// <param name="DefaultValue">The optional default value.</param>
internal readonly record struct PropertyToGenerate(
    string Name,
    string FieldName,
    Microsoft.CodeAnalysis.CSharp.Syntax.TypeSyntax Type,
    PropertyMetadata Metadata,
    Microsoft.CodeAnalysis.CSharp.SyntaxKind Accessibility,
    TypedConstant? DefaultValue)
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

        var metadata = PropertyMetadata.None;

        if (type.IsPrimitiveOrNullablePrimitive)
        {
            metadata |= PropertyMetadata.Primitive;
        }

        if (propertySymbol.IsReadOnly)
        {
            metadata |= PropertyMetadata.ReadOnly;
        }

        if (type.IsCollection(collectionTypeSymbol))
        {
            metadata |= PropertyMetadata.Collection;
        }

        if (type.IsDictionary(dictionaryTypeSymbol))
        {
            metadata |= PropertyMetadata.Dictionary;
        }

        if (!type.IsValueType && (propertySymbol.NullableAnnotation is not NullableAnnotation.None || propertySymbol.Type.NullableAnnotation is not NullableAnnotation.None))
        {
            metadata |= PropertyMetadata.Nullable;
        }

        var accessibility = GetAccessibility(propertySymbol, metadata);
        var defaultValue = GetDefaultValue(propertySymbol);
        return new(name, fieldName, typeSyntax, metadata, accessibility, defaultValue);

        static Microsoft.CodeAnalysis.CSharp.SyntaxKind GetAccessibility(IPropertySymbol propertySymbol, PropertyMetadata metadata)
        {
            if (metadata.HasFlag(PropertyMetadata.ReadOnly) && metadata.HasFlag(PropertyMetadata.Collection) && propertySymbol.GetMethod is { DeclaredAccessibility: var getDeclaredAccessibility })
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

        static TypedConstant? GetDefaultValue(IPropertySymbol propertySymbol)
        {
            foreach (var attribute in propertySymbol.GetAttributes())
            {
                if (attribute is { AttributeClass.Name: { } name }
                    && (StringComparer.Ordinal.Equals(name, TypeNames.DefaultValueAttributeShortName) ||
                        StringComparer.Ordinal.Equals(name, TypeNames.DefaultValueAttributeLongName))
                    && attribute.ConstructorArguments is [var defaultValueConstant])
                {
                    return defaultValueConstant;
                }
            }

            return default;
        }
    }

    /// <inheritdoc/>
    public bool Equals(PropertyToGenerate other) => StringComparer.Ordinal.Equals(this.Name, other.Name)
                                                    && StringComparer.Ordinal.Equals(this.FieldName, other.FieldName)
                                                    && StringComparer.Ordinal.Equals(this.Type.ToFullString(), other.Type.ToFullString())
                                                    && this.Metadata == other.Metadata
                                                    && this.Accessibility == other.Accessibility
                                                    && this.DefaultValue.GetValueOrDefault().Equals(other.DefaultValue.GetValueOrDefault());

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        var hash = 0;
        hash += StringComparer.Ordinal.GetHashCode(this.Name);
        hash += 19 + StringComparer.Ordinal.GetHashCode(this.Name);
        hash += 38 + StringComparer.Ordinal.GetHashCode(this.Type.ToFullString());
        hash += 57 + this.Metadata.GetHashCode();
        hash += 76 + this.Accessibility.GetHashCode();
        if (this.DefaultValue is { } defaultValue)
        {
            hash += 95 + defaultValue.GetHashCode();
        }

        return hash;
    }

    /// <summary>
    /// Tries to get the builder for this property.
    /// </summary>
    /// <param name="builders">The potential builders.</param>
    /// <param name="builder">The builder if found.</param>
    /// <returns><see langword="true"/> if a builder is found; otherwise <see langword="false"/>.</returns>
    public bool TryGetBuilder(IEnumerable<BuilderToGenerate> builders, out BuilderToGenerate builder)
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