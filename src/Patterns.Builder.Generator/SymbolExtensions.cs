// -----------------------------------------------------------------------
// <copyright file="SymbolExtensions.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

#pragma warning disable IDE0130, CheckNamespace
namespace Microsoft.CodeAnalysis;
#pragma warning restore IDE0130, CheckNamespace

#pragma warning disable RedundantNameQualifier
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
#pragma warning restore RedundantNameQualifier

#pragma warning disable RCS1263, SA1101

/// <summary>
/// The symbol extensions.
/// </summary>
internal static class SymbolExtensions
{
    /// <content>
    /// The <see cref="Type"/> extensions.
    /// </content>
    /// <param name="type">The type.</param>
    extension(Type type)
    {
        /// <summary>
        /// Converts the type to the <see cref="TypeSyntax"/>.
        /// </summary>
        /// <param name="parameters">The type parameters.</param>
        /// <returns>The type syntax.</returns>
        /// <exception cref="ArgumentOutOfRangeException">The parameters are the wrong length.</exception>
        public NameSyntax ToTypeSyntax(IEnumerable<TypeSyntax> parameters)
        {
            if (!type.IsGenericTypeDefinition)
            {
                return SyntaxFactory.QualifiedName(type.FullName);
            }

            var index = type.Name.IndexOf('`');
            var name = type.Name[..index];
            var count = int.Parse(type.Name[(index + 1)..], System.Globalization.CultureInfo.InvariantCulture);

            var parameterList = SeparatedList(parameters);
            if (count != parameterList.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(parameters));
            }

            return QualifiedName(
                SyntaxFactory.QualifiedName(type.Namespace),
                GenericName(Identifier(name), TypeArgumentList(parameterList)));
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
            {
                SpecialType: SpecialType.System_Boolean or
                    SpecialType.System_Char or
                    SpecialType.System_SByte or
                    SpecialType.System_Byte or
                    SpecialType.System_Int16 or
                    SpecialType.System_UInt16 or
                    SpecialType.System_Int32 or
                    SpecialType.System_UInt32 or
                    SpecialType.System_Int64 or
                    SpecialType.System_UInt64 or
                    SpecialType.System_Single or
                    SpecialType.System_Double,
            }

                => true,
            { SpecialType: SpecialType.System_Enum } or { TypeKind: TypeKind.Enum } => true,
            INamedTypeSymbol { NullableAnnotation: NullableAnnotation.Annotated, IsValueType: true, TypeArguments: [var typeArgument] } => typeArgument.IsPrimitiveOrNullablePrimitive,
            INamedTypeSymbol { SpecialType: SpecialType.System_Nullable_T, TypeArguments: [var typeArgument] } => typeArgument.IsPrimitiveOrNullablePrimitive,
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
                INamedTypeSymbol { NullableAnnotation: NullableAnnotation.Annotated, TypeArguments: [var typeArgument] } => NullableType(typeArgument.ToType()),
                INamedTypeSymbol { SpecialType: SpecialType.System_Nullable_T, TypeArguments: [var typeArgument] } => NullableType(typeArgument.ToType()),

                // Array
                IArrayTypeSymbol { NullableAnnotation: NullableAnnotation.Annotated, ElementType: { } elementType } => NullableType(GetArray(elementType)),
                IArrayTypeSymbol { ElementType: { } elementType } => GetArray(elementType),

                // Pointers
                IPointerTypeSymbol { PointedAtType: { } pointedAtType } => PointerType(pointedAtType.ToType()),

                // Non-special
                INamedTypeSymbol namedTypeSymbol => GetFullName(namedTypeSymbol),

                // anything not supported yet
                _ => throw new NotSupportedException(),
            };

            static ArrayTypeSyntax GetArray(ITypeSymbol elementType)
            {
                return ArrayType(elementType.ToType())
                    .WithRankSpecifiers(
                        SingletonList(
                            ArrayRankSpecifier(
                                SingletonSeparatedList<ExpressionSyntax>(
                                    OmittedArraySizeExpression()))));
            }

            static TypeSyntax GetFullName(INamedTypeSymbol type)
            {
                var @namespace = GetNamespace(type.ContainingNamespace);

                // is this generic?
                return type.IsGenericType
                    ? QualifiedName(@namespace, GenericName(type.Name).WithTypeArgumentList(TypeArgumentList(GetTypeArguments(type))))
                    : QualifiedName(@namespace, IdentifierName(type.Name));

                static NameSyntax GetNamespace(INamespaceSymbol? namespaceSymbol)
                {
                    return GetNamespaceParts(namespaceSymbol)
                        .Reverse()
                        .Select(IdentifierName)
                        .ToQualifiedName();

                    static IEnumerable<string> GetNamespaceParts(INamespaceSymbol? namespaceSymbol)
                    {
                        while (namespaceSymbol is { Name: { Length: > 0 } name })
                        {
                            yield return name;
                            namespaceSymbol = namespaceSymbol.ContainingNamespace;
                        }
                    }
                }

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
                        return SingletonSeparatedList(first.ToType());
                    }

                    var arguments = new List<TypeSyntax> { first.ToType(), typeArguments.Current.ToType(), };

                    while (typeArguments.MoveNext())
                    {
                        arguments.Add(typeArguments.Current.ToType());
                    }

                    return SeparatedList(arguments);
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

    extension(IMethodSymbol methodSymbol)
    {
        /// <summary>
        /// Gets the parameter list.
        /// </summary>
        /// <returns>The parameter list.</returns>
        public ParameterListSyntax GetParameterList()
        {
            return ParameterList(SeparatedList(GetParameters(methodSymbol)));

            static IEnumerable<ParameterSyntax> GetParameters(IMethodSymbol methodSymbol)
            {
                return methodSymbol.Parameters.Select(parameter =>
                    Parameter(
                            Identifier(parameter.Name))
                        .WithType(parameter.Type.ToType()));
            }
        }

        /// <summary>
        /// Gets the object creation.
        /// </summary>
        /// <returns>The object creation list.</returns>
        public ObjectCreationExpressionSyntax GetObjectCreation()
        {
            if (methodSymbol is { MethodKind: MethodKind.Constructor, ReceiverType: { } receiverType })
            {
                return ObjectCreationExpression(
                        SyntaxFactory.QualifiedName(receiverType.ToString()))
                    .WithArgumentList(ArgumentList(SeparatedList(GetArguments(methodSymbol))));
            }

            throw new InvalidOperationException();

            static IEnumerable<ArgumentSyntax> GetArguments(IMethodSymbol methodSymbol)
            {
                return Enumerable.Select(methodSymbol.Parameters, parameter => Argument(IdentifierName(parameter.Name)));
            }
        }
    }
}