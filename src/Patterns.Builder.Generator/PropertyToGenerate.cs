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
    ExpressionSyntax? DefaultValue,
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

        static ExpressionSyntax? GetDefaultValue(IPropertySymbol propertySymbol)
        {
            foreach (var attribute in propertySymbol.GetAttributes())
            {
                if (attribute is { AttributeClass.Name: { } name }
                    && (StringComparer.Ordinal.Equals(name, TypeNames.DefaultValueAttributeShortName) ||
                        StringComparer.Ordinal.Equals(name, TypeNames.DefaultValueAttributeLongName)))
                {
                    if (attribute.ConstructorArguments is [var defaultValueConstant])
                    {
                        return CreateExpressionFromTypedConstant(defaultValueConstant);
                    }

                    // converter based
                    if (attribute.ConstructorArguments is [var typeArgument, var stringArgument])
                    {
                        // this needs to be an expression
                        const string defaultValueAttribute = nameof(defaultValueAttribute);
                        return SyntaxFactory.ParenthesizedLambdaExpression()
                            .WithBlock(
                                SyntaxFactory.Block(
                                    SyntaxFactory.LocalDeclarationStatement(
                                        SyntaxFactory.VariableDeclaration(
                                                SyntaxFactory.IdentifierName(
                                                    SyntaxFactory.Identifier(
                                                        SyntaxFactory.TriviaList(),
                                                        SyntaxKind.VarKeyword,
                                                        SyntaxFacts.GetText(SyntaxKind.VarKeyword),
                                                        SyntaxFacts.GetText(SyntaxKind.VarKeyword),
                                                        SyntaxFactory.TriviaList())))
                                            .WithVariables(
                                                SyntaxFactory.SingletonSeparatedList(
                                                    SyntaxFactory.VariableDeclarator(
                                                            SyntaxFactory.Identifier(defaultValueAttribute))
                                                        .WithInitializer(
                                                            SyntaxFactory.EqualsValueClause(
                                                                SyntaxFactory.ObjectCreationExpression(
                                                                        typeof(System.ComponentModel.DefaultValueAttribute).ToTypeSyntax([]))
                                                                    .WithArgumentList(
                                                                        SyntaxFactory.ArgumentList(
                                                                            SyntaxFactory.SeparatedList<ArgumentSyntax>(
                                                                            [
                                                                                SyntaxFactory.Argument(
                                                                                    CreateExpressionFromTypedConstant(typeArgument)),
                                                                                SyntaxFactory.Argument(
                                                                                    CreateExpressionFromTypedConstant(stringArgument)),
                                                                            ])))))))),
                                    SyntaxFactory.ReturnStatement(
                                        SyntaxFactory.CastExpression(
                                            propertySymbol.Type.ToType(),
                                            SyntaxFactory.MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                SyntaxFactory.IdentifierName(defaultValueAttribute),
                                                SyntaxFactory.IdentifierName(nameof(System.ComponentModel.DefaultValueAttribute.Value)))))));
                    }
                }
            }

            return default;
        }
    }

    /// <inheritdoc/>
    public bool Equals(PropertyToGenerate other)
    {
        return StringComparer.Ordinal.Equals(this.Name, other.Name)
               && StringComparer.Ordinal.Equals(this.FieldName, other.FieldName)
               && CheckSyntaxNodesViaToString(this.Type, other.Type)
               && this.Metadata == other.Metadata
               && this.Accessibility == other.Accessibility
               && CheckSyntaxNodesViaToString(this.DefaultValue, other.DefaultValue)
               && this.Constructors.Equals(other.Constructors);

        static bool CheckSyntaxNodesViaToString(SyntaxNode? left, SyntaxNode? right)
        {
            return (left, right) switch
            {
                (not null, not null) => StringComparer.Ordinal.Equals(left.ToFullString(), right.ToFullString()),
                (null, null) => true,
                _ => false,
            };
        }
    }

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

    private static ExpressionSyntax CreateExpressionFromTypedConstant(TypedConstant constant)
    {
        return constant switch
        {
            { IsNull: true } => SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression),
            { Kind: TypedConstantKind.Primitive } => GetLiteralExpression(constant),
            { Kind: TypedConstantKind.Enum } => CreateEnumExpression(constant),
            { Kind: TypedConstantKind.Type } => CreateTypeOfExpression(constant),
            _ => throw new NotSupportedException($"Unsupported TypedConstantKind: {constant.Kind}"),
        };

        static ExpressionSyntax CreateEnumExpression(TypedConstant constant)
        {
            if (constant.Type is not INamedTypeSymbol { TypeKind: TypeKind.Enum } enumType)
            {
                throw new InvalidOperationException($"{nameof(TypedConstant)} is not an enum type.");
            }

            // Try to find the enum member with the matching constant value
            var matchingField = enumType
                .GetMembers()
                .OfType<IFieldSymbol>()
                .FirstOrDefault(f => f.HasConstantValue && Equals(f.ConstantValue, constant.Value));

            if (matchingField != null)
            {
                // MyEnum.MemberName
                return SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    SyntaxFactory.QualifiedName(enumType.ToString()),
                    SyntaxFactory.IdentifierName(matchingField.Name));
            }

            // If no matching field found, fallback to casting the numeric value
            return SyntaxFactory.CastExpression(
                SyntaxFactory.QualifiedName(enumType.ToString()),
                GetLiteralExpression(constant));
        }

        static ExpressionSyntax CreateTypeOfExpression(TypedConstant constant)
        {
            if (constant.Value is { } value)
            {
                return SyntaxFactory.TypeOfExpression(
                    SyntaxFactory.QualifiedName(value.ToString()));
            }

            throw new InvalidOperationException($"{nameof(TypedConstant)} is not a typeof.");
        }

        static LiteralExpressionSyntax GetLiteralExpression(TypedConstant constant)
        {
            return SyntaxFactory.LiteralExpression(
                GetLiteralExpressionKind(constant.Value),
                GetLiteralExpressionToken(constant.Value));

            static SyntaxKind GetLiteralExpressionKind(object? value) =>
                value switch
                {
                    int or double or float => SyntaxKind.NumericLiteralExpression,
                    true => SyntaxKind.TrueLiteralExpression,
                    false => SyntaxKind.FalseLiteralExpression,
                    char => SyntaxKind.CharacterLiteralExpression,
                    null => SyntaxKind.NullLiteralExpression,
                    _ => SyntaxKind.StringLiteralExpression,
                };

            static SyntaxToken GetLiteralExpressionToken(object? value) =>
                value switch
                {
                    sbyte i => SyntaxFactory.Literal(i),
                    byte i => SyntaxFactory.Literal(i),
                    short i => SyntaxFactory.Literal(i),
                    ushort i => SyntaxFactory.Literal(i),
                    int i => SyntaxFactory.Literal(i),
                    uint i => SyntaxFactory.Literal(i),
                    long i => SyntaxFactory.Literal(i),
                    ulong i => SyntaxFactory.Literal(i),
                    float f => SyntaxFactory.Literal(f),
                    double f => SyntaxFactory.Literal(f),
                    decimal f => SyntaxFactory.Literal(f),
                    char c => SyntaxFactory.Literal(c),
                    string s => SyntaxFactory.Literal(s),
                    not null => SyntaxFactory.Literal(value.ToString()),
                    _ => SyntaxFactory.Token(SyntaxKind.NullKeyword),
                };
        }
    }
}