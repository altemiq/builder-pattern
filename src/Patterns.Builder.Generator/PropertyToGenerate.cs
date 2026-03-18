// -----------------------------------------------------------------------
// <copyright file="PropertyToGenerate.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.Patterns.Builder.Generator;

using System.Collections.Immutable;
using Humanizer;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

/// <summary>
/// The property to generate.
/// </summary>
/// <param name="Name">The name.</param>
/// <param name="FieldName">The field name.</param>
/// <param name="Type">The type.</param>
/// <param name="Metadata">The property metadata.</param>
/// <param name="Accessibility">The accessibility.</param>
/// <param name="DefaultValue">The optional default value.</param>
/// <param name="Constructors">The optional constructors.</param>
internal readonly record struct PropertyToGenerate(
    string Name,
    string FieldName,
    TypeSyntax Type,
    PropertyMetadata Metadata,
    SyntaxKind Accessibility,
    TypedConstant? DefaultValue,
    IImmutableList<IMethodSymbol> Constructors)
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
        var fieldName = SyntaxFacts.EscapeKeyword(name.Camelize());

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

        var collection = false;
        if (type.IsCollection(collectionTypeSymbol))
        {
            collection = true;
            metadata |= PropertyMetadata.Collection;
        }

        if (type.IsDictionary(dictionaryTypeSymbol))
        {
            collection = false;
            metadata |= PropertyMetadata.Dictionary;
        }

        if (!type.IsValueType && (propertySymbol.NullableAnnotation is not NullableAnnotation.None || propertySymbol.Type.NullableAnnotation is not NullableAnnotation.None))
        {
            metadata |= PropertyMetadata.Nullable;
        }

        var accessibility = GetAccessibility(propertySymbol, metadata);
        var defaultValue = GetDefaultValue(propertySymbol);
        if (GetInstanceConstructors(propertySymbol.Type, collection) is not { Count: not 0 } instanceConstructors)
        {
            return new(name, fieldName, typeSyntax, metadata, accessibility, defaultValue, []);
        }

        IImmutableList<IMethodSymbol> constructors = [..instanceConstructors.Where(static method =>
            {
                if (method.Parameters.IsEmpty)
                {
                    return false;
                }

                // exclude copy constructors
                if (method.Parameters is [var single]
                    && SymbolEqualityComparer.Default.Equals(single.Type, method.ReceiverType))
                {
                    return false;
                }

                // exclude unsafe methods
                return !method.Parameters.Any(p => p.Type is IPointerTypeSymbol);
            })
            .OrderBy(static method => method.GetDocumentationCommentId(), StringComparer.Ordinal),
        ];

        constructors = constructors.Count is 0
            ? ImmutableArray<IMethodSymbol>.Empty
            : constructors.WithValueSemantics(SymbolEqualityComparer.IncludeNullability);

        return new(name, fieldName, typeSyntax, metadata, accessibility, defaultValue, constructors);

        static ICollection<IMethodSymbol> GetInstanceConstructors(ITypeSymbol type, bool collection)
        {
            while (true)
            {
                // if this is a nullable value type, then get the value type
                if (type.NullableAnnotation is NullableAnnotation.Annotated
                    && type.IsValueType
                    && type is INamedTypeSymbol { TypeArguments: [var nullableTypeArgument] })
                {
                    type = nullableTypeArgument;
                    continue;
                }

                if (collection && type is INamedTypeSymbol { TypeArguments: [var collectionTypeArgument] })
                {
                    type = collectionTypeArgument;
                    continue;
                }

                return type is INamedTypeSymbol { InstanceConstructors: var instanceConstructors }
                    ? instanceConstructors
                    : [];
            }
        }

        static SyntaxKind GetAccessibility(IPropertySymbol propertySymbol, PropertyMetadata metadata)
        {
            if (metadata.HasFlag(PropertyMetadata.ReadOnly) && metadata.HasFlag(PropertyMetadata.Collection) && propertySymbol.GetMethod is { DeclaredAccessibility: var getDeclaredAccessibility })
            {
                return getDeclaredAccessibility switch
                {
                    Microsoft.CodeAnalysis.Accessibility.Internal => SyntaxKind.InternalKeyword,
                    Microsoft.CodeAnalysis.Accessibility.Public => SyntaxKind.PublicKeyword,
                    _ => SyntaxKind.PrivateKeyword,
                };
            }

            if (propertySymbol.SetMethod is { DeclaredAccessibility: var setDeclaredAccessibility })
            {
                return setDeclaredAccessibility switch
                {
                    Microsoft.CodeAnalysis.Accessibility.Internal => SyntaxKind.InternalKeyword,
                    Microsoft.CodeAnalysis.Accessibility.Public => SyntaxKind.PublicKeyword,
                    _ => SyntaxKind.PrivateKeyword,
                };
            }

            return SyntaxKind.PrivateKeyword;
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
                                                    && this.DefaultValue.GetValueOrDefault().Equals(other.DefaultValue.GetValueOrDefault())
                                                    && this.Constructors.Equals(other.Constructors);

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        const int multiplier = 19;
        var hash = UpdateHashCode(multiplier, StringComparer.Ordinal.GetHashCode(this.Name));
        hash = UpdateHashCode(hash, StringComparer.Ordinal.GetHashCode(this.FieldName));
        hash = UpdateHashCode(hash, StringComparer.Ordinal.GetHashCode(this.Type.ToFullString()));
        hash = UpdateHashCode(hash, this.Metadata.GetHashCode());
        hash = UpdateHashCode(hash, this.Accessibility.GetHashCode());
        if (this.DefaultValue is { } defaultValue)
        {
            hash = UpdateHashCode(hash, defaultValue.GetHashCode());
        }

        return UpdateHashCode(hash, this.Constructors.GetHashCode());

        static int UpdateHashCode(int hash, int code)
        {
            return (hash * multiplier) + code;
        }
    }

    /// <summary>
    /// Tries to get the builder for this property.
    /// </summary>
    /// <param name="builders">The potential builders.</param>
    /// <param name="builder">The builder if found.</param>
    /// <returns><see langword="true"/> if a builder is found; otherwise <see langword="false"/>.</returns>
    public bool TryGetBuilder(ICollection<BuilderToGenerate> builders, out BuilderToGenerate builder)
    {
        return (this.Type is QualifiedNameSyntax { Right: GenericNameSyntax { TypeArgumentList.Arguments: [var typeArgument] } } && TryGetBuilderCore(builders, typeArgument, out builder))
            || TryGetBuilderCore(builders, this.Type, out builder);

        static bool TryGetBuilderCore(IEnumerable<BuilderToGenerate> builders, TypeSyntax type, out BuilderToGenerate builder)
        {
            var typeName = type.ToFullString();
            builder = builders.FirstOrDefault(potentialBuilder => StringComparer.Ordinal.Equals(potentialBuilder.FullyQualifiedClassName, typeName));
            return builder.ClassName is not null;
        }
    }
}