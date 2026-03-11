// -----------------------------------------------------------------------
// <copyright file="SymbolExtensions.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.Patterns.Builder.Generator;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

#pragma warning disable RCS1263, SA1101

/// <summary>
/// The symbol extensions.
/// </summary>
internal static class SymbolExtensions
{
    /// <content>
    /// The <see cref="MemberDeclarationSyntax"/> extensions.
    /// </content>
    /// <param name="syntax"></param>
    extension(MemberDeclarationSyntax syntax)
    {
        /// <summary>
        /// Gets a value indicating whether this instance is partial.
        /// </summary>
        public bool IsPartial => syntax.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword));
    }

    extension(Type type)
    {
        public NameSyntax ToTypeSyntax(params TypeSyntax[] parameters)
        {
            if (type.IsGenericTypeDefinition)
            {
                var index = type.Name.IndexOf('`');
                var name = type.Name[..index];
                var count = int.Parse(type.Name[(index + 1)..], System.Globalization.CultureInfo.InvariantCulture);
                if (count != parameters.Length)
                {
                    throw new ArgumentOutOfRangeException(nameof(parameters));
                }

                return QualifiedName(
                    NameHelpers.GetQualifiedName(type.Namespace),
                    GenericName(
                        Identifier(name))
                    .WithTypeArgumentList(
                        TypeArgumentList(SeparatedList(parameters))));
            }

            throw new NotSupportedException();
        }

        public NameSyntax ToTypeSyntax()
        {
            if (type.IsGenericType)
            {
                // send this back as a generic type
                return QualifiedName(
                    NameHelpers.GetQualifiedName(type.Namespace),
                    GenericName(
                        Identifier(type.Name))
                    .WithTypeArgumentList(
                        TypeArgumentList(
                            GetArguments(type.GenericTypeArguments))));

                static SeparatedSyntaxList<TypeSyntax> GetArguments(Type[] types)
                {
                    if (types.Length == 0)
                    {
                        return [];
                    }

                    if (types.Length is 1)
                    {
                        return SingletonSeparatedList<TypeSyntax>(types[0].ToTypeSyntax());
                    }

                    return SeparatedList<TypeSyntax>(types.Select(ToTypeSyntax));
                }
            }

            return NameHelpers.GetQualifiedName(type.FullName);
        }
    }

    /// <content>
    /// The <see cref="ITypeSymbol"/> extensions.
    /// </content>
    /// <param name="type">The type symbol.</param>
    extension(ITypeSymbol type)
    {
        /// <summary>
        /// Gets a value indicating whether this instance is a primitive, or a nullable primitive.
        /// </summary>
        public bool IsPrimitiveOrNullablePrimitive => type switch
        {
            { SpecialType: SpecialType.System_Boolean or SpecialType.System_Char or SpecialType.System_SByte or SpecialType.System_Byte or SpecialType.System_Int16 or SpecialType.System_UInt16 or SpecialType.System_Int32 or SpecialType.System_UInt32 or SpecialType.System_Int64 or SpecialType.System_UInt64 or SpecialType.System_Single or SpecialType.System_Double } => true,
            INamedTypeSymbol { NullableAnnotation: NullableAnnotation.Annotated, IsValueType: true } namedTypeSymbol => namedTypeSymbol.TypeArguments[0].IsPrimitiveOrNullablePrimitive,
            INamedTypeSymbol { SpecialType: SpecialType.System_Nullable_T } namedTypeSymbol => namedTypeSymbol.TypeArguments[0].IsPrimitiveOrNullablePrimitive,
            _ => false,
        };

        /// <summary>
        /// Gets the type syntax from the type symbol.
        /// </summary>
        /// <returns>The type syntax.</returns>
        public TypeSyntax ToType()
        {
            return type switch
            {
                { SpecialType: SpecialType.System_Object } => PredefinedType(Token(SyntaxKind.ObjectKeyword)),

                // Bytes
                { SpecialType: SpecialType.System_Boolean } => PredefinedType(Token(SyntaxKind.BoolKeyword)),
                { SpecialType: SpecialType.System_Char } => PredefinedType(Token(SyntaxKind.CharKeyword)),
                { SpecialType: SpecialType.System_SByte } => PredefinedType(Token(SyntaxKind.SByteKeyword)),
                { SpecialType: SpecialType.System_Byte } => PredefinedType(Token(SyntaxKind.ByteKeyword)),

                // Integer
                { SpecialType: SpecialType.System_Int16 } => PredefinedType(Token(SyntaxKind.ShortKeyword)),
                { SpecialType: SpecialType.System_UInt16 } => PredefinedType(Token(SyntaxKind.UShortKeyword)),
                { SpecialType: SpecialType.System_Int32 } => PredefinedType(Token(SyntaxKind.IntKeyword)),
                { SpecialType: SpecialType.System_UInt32 } => PredefinedType(Token(SyntaxKind.UIntKeyword)),
                { SpecialType: SpecialType.System_Int64 } => PredefinedType(Token(SyntaxKind.LongKeyword)),
                { SpecialType: SpecialType.System_UInt64 } => PredefinedType(Token(SyntaxKind.ULongKeyword)),

                // Floating-point
                { SpecialType: SpecialType.System_Decimal } => PredefinedType(Token(SyntaxKind.DecimalKeyword)),
                { SpecialType: SpecialType.System_Single } => PredefinedType(Token(SyntaxKind.FloatKeyword)),
                { SpecialType: SpecialType.System_Double } => PredefinedType(Token(SyntaxKind.DoubleKeyword)),

                // String
                { SpecialType: SpecialType.System_String, NullableAnnotation: NullableAnnotation.Annotated } => NullableType(PredefinedType(Token(SyntaxKind.StringKeyword))),
                { SpecialType: SpecialType.System_String } => PredefinedType(Token(SyntaxKind.StringKeyword)),

                // Nullable
                INamedTypeSymbol { NullableAnnotation: NullableAnnotation.Annotated } namedTypeSymbol => NullableType(ToType(namedTypeSymbol.TypeArguments[0])),
                INamedTypeSymbol { SpecialType: SpecialType.System_Nullable_T } namedTypeSymbol => NullableType(ToType(namedTypeSymbol.TypeArguments[0])),

                // Non-special
                INamedTypeSymbol namedTypeSymbol => GetFullName(namedTypeSymbol),

                _ => throw new NotSupportedException(),
            };

            static TypeSyntax GetFullName(INamedTypeSymbol type)
            {
                var @namespace = GetNamespace(type.ContainingNamespace);

                // is this generic?
                if (type.IsGenericType)
                {
                    // add the values on the end
                    var genericName = GenericName(type.Name)
                        .WithTypeArgumentList(TypeArgumentList(GetTypeArguments(type)));

                    return QualifiedName(@namespace, genericName);

                    static SeparatedSyntaxList<TypeSyntax> GetTypeArguments(INamedTypeSymbol type)
                    {
                        var typeArguments = type.TypeArguments.GetEnumerator();

                        if (!typeArguments.MoveNext())
                        {
                            throw new InvalidOperationException();
                        }

                        var first = typeArguments.Current;

                        if (!typeArguments.MoveNext())
                        {
                            return SingletonSeparatedList<TypeSyntax>(first.ToType());
                        }

                        var arguments = new List<TypeSyntax>
                        {
                            first.ToType(),
                            typeArguments.Current.ToType(),
                        };

                        while (typeArguments.MoveNext())
                        {
                            arguments.Add(typeArguments.Current.ToType());
                        }

                        return SeparatedList(arguments);
                    }
                }

                return QualifiedName(@namespace, IdentifierName(type.Name));

                static NameSyntax GetNamespace(INamespaceSymbol? namespaceSymbol)
                {
                    var sections = new List<string>();

                    while (namespaceSymbol is { Name: { Length: > 0 } name })
                    {
                        sections.Insert(0, name);
                        namespaceSymbol = namespaceSymbol.ContainingNamespace;
                    }

                    return NameHelpers.GetQualifiedName(sections.Select(IdentifierName));
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is a collection.
        /// </summary>
        public bool IsCollection(ITypeSymbol? collectionType) => type switch
        {
            { SpecialType: SpecialType.System_Collections_Generic_ICollection_T } => true,
            { OriginalDefinition: { } originalDefinition } when SymbolEqualityComparer.Default.Equals(originalDefinition, collectionType) => true,
            { AllInterfaces: { } interfaces } when interfaces.Select(i => i.OriginalDefinition).Contains(collectionType, SymbolEqualityComparer.Default) => true,
            _ => false,
        };

        /// <summary>
        /// Gets a value indicating whether this instance is a dictionary.
        /// </summary>
        public bool IsDictionary(ITypeSymbol? dictionaryType) => type switch
        {
            { OriginalDefinition: { } originalDefinition } when SymbolEqualityComparer.Default.Equals(originalDefinition, dictionaryType) => true,
            { AllInterfaces: { } interfaces } when interfaces.Select(i => i.OriginalDefinition).Contains(dictionaryType, SymbolEqualityComparer.Default) => true,
            _ => false,
        };
    }
}