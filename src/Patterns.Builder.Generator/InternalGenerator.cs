// -----------------------------------------------------------------------
// <copyright file="InternalGenerator.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.Patterns.Builder.Generator;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

/// <summary>
/// The internal generator.
/// </summary>
internal static partial class InternalGenerator
{
    private const string Key = "key";
    private const string Value = "value";
    private const string Item = "item";
    private const string KeyValuePair = "kvp";
    private const string CreateBuilderMethod = "CreateBuilder";
    private const string BuildMethod = "Build";
    private const string ActionParameterName = "action";
    private const string BuilderVariableName = "builder";

    private static readonly XmlElementSyntax BuilderReturn = XmlReturnsElement(XmlText("The builder for chaining."));
    private static readonly IdentifierNameSyntax VarIdentifierName =
        IdentifierName(
            Identifier(
                TriviaList(),
                SyntaxKind.VarKeyword,
                SyntaxFacts.GetText(SyntaxKind.VarKeyword),
                SyntaxFacts.GetText(SyntaxKind.VarKeyword),
                TriviaList()));

    /// <summary>
    /// Generates the builder.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="builders">The possible builders.</param>
    /// <param name="useCollectionExpressions">Set to <see langword="true"/> to use collection expressions.</param>
    /// <returns>The generated builder.</returns>
    public static CompilationUnitSyntax GenerateNestedBuilder(BuilderToGenerate context, System.Collections.Immutable.ImmutableArray<BuilderToGenerate> builders, bool useCollectionExpressions)
    {
        var qualifiedClassName = SyntaxFactory.QualifiedName(context.FullyQualifiedClassName);
        return InNamespace(
            context.Namespace,
            context.Properties.Any(static property => property.Metadata.HasFlag(PropertyMetadata.Nullable)),
            SingletonList<MemberDeclarationSyntax>(
            GetPartialDeclaration(context)
            .WithModifiers(TokenList(Token(SyntaxKind.PartialKeyword)))
            .WithLeadingTrivia(
            Trivia(
                DocumentationComment(
                    XmlMultiLineElement(
                        "content",
                        List<XmlNodeSyntax>(
                        [
                            XmlText("The "),
                            XmlSeeElement(
                                TypeCref(
                                    qualifiedClassName)),
                            XmlText(" builder methods."),
                        ])),
                    XmlText(XmlTextNewLine(Constants.NewLine, continueXmlDocumentationComment: false)))))
            .WithMembers(
                List<MemberDeclarationSyntax>(
                    [
                    MethodDeclaration(
                        IdentifierName(context.BuilderName),
                        Identifier(CreateBuilderMethod))
                    .WithModifiers(
                        TokenList(
                            Token(SyntaxKind.PublicKeyword),
                            Token(SyntaxKind.StaticKeyword)))
                    .WithLeadingTrivia(
                        Trivia(
                            DocumentationComment(
                                XmlSummaryElement(
                                    XmlText("Creates a new builder for a "),
                                    XmlSeeElement(
                                        TypeCref(
                                            qualifiedClassName)),
                                    XmlText(" instance.")),
                                XmlText(XmlTextNewLine(Constants.NewLine)),
                                XmlReturnsElement(
                                    XmlText("The builder for a "),
                                    XmlSeeElement(
                                        TypeCref(
                                            qualifiedClassName)),
                                    XmlText(" instance.")),
                                XmlText(XmlTextNewLine(Constants.NewLine, continueXmlDocumentationComment: false)))))
                    .WithBody(
                        Block(
                            SingletonList<StatementSyntax>(
                                ReturnStatement(
                                    ObjectCreationExpression(
                                        IdentifierName(context.BuilderName))
                                    .WithArgumentList(
                                        ArgumentList()))))),
                    GenerateBuilder(context, [SyntaxKind.PublicKeyword, SyntaxKind.SealedKeyword, SyntaxKind.PartialKeyword], builders, useCollectionExpressions),
                ]))));

        static TypeDeclarationSyntax GetPartialDeclaration(BuilderToGenerate context)
        {
            return context.ClassDefinition switch
            {
                SyntaxKind.ClassDeclaration => ClassDeclaration(context.ClassName),
                SyntaxKind.StructDeclaration => StructDeclaration(context.ClassName),
                SyntaxKind.RecordDeclaration => GetRecordDeclaration(SyntaxKind.RecordDeclaration, context.ClassName, SyntaxKind.ClassKeyword),
                SyntaxKind.RecordStructDeclaration => GetRecordDeclaration(SyntaxKind.RecordStructDeclaration, context.ClassName, SyntaxKind.StructKeyword),
                _ => throw new InvalidOperationException(),
            };

            static RecordDeclarationSyntax GetRecordDeclaration(SyntaxKind kind, string name, SyntaxKind classOrStructKeyword)
            {
                return RecordDeclaration(kind, Token(SyntaxKind.RecordKeyword), name)
                        .WithClassOrStructKeyword(Token(classOrStructKeyword))
                        .WithOpenBraceToken(Token(SyntaxKind.OpenBraceToken))
                        .WithCloseBraceToken(Token(SyntaxKind.CloseBraceToken));
            }
        }
    }

    /// <summary>
    /// Generates the builder.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="builders">The possible builders.</param>
    /// <param name="useCollectionExpressions">Set to <see langword="true"/> to use collection expressions.</param>
    /// <returns>The generated builder.</returns>
    public static CompilationUnitSyntax GenerateExternalBuilder(BuilderToGenerate context, System.Collections.Immutable.ImmutableArray<BuilderToGenerate> builders, bool useCollectionExpressions) => InNamespace(
            context.Namespace,
            context.Properties.Any(static property => property.Metadata.HasFlag(PropertyMetadata.Nullable)),
            SingletonList<MemberDeclarationSyntax>(GenerateBuilder(context, [SyntaxKind.SealedKeyword, SyntaxKind.PartialKeyword], builders, useCollectionExpressions)));

    private static CompilationUnitSyntax InNamespace(string @namespace, bool enableNullable, SyntaxList<MemberDeclarationSyntax> members)
    {
        return CompilationUnit()
            .WithMembers(
            SingletonList<MemberDeclarationSyntax>(
                NamespaceDeclaration(SyntaxFactory.QualifiedName(@namespace))
                .WithLeadingTrivia(GetLeadingTrivia(enableNullable))
                .WithMembers(members)));

        static IEnumerable<SyntaxTrivia> GetLeadingTrivia(bool enableNullable)
        {
            yield return Comment("// <autogenerated />");
            if (enableNullable)
            {
                yield return Trivia(
                            NullableDirectiveTrivia(
                                Token(SyntaxKind.EnableKeyword),
                                isActive: true));
            }
        }
    }

    private static ClassDeclarationSyntax GenerateBuilder(
        BuilderToGenerate context,
        IEnumerable<SyntaxKind> modifiers,
        System.Collections.Immutable.ImmutableArray<BuilderToGenerate> builders,
        bool useCollectionExpressions)
    {
        return ClassDeclaration(context.BuilderName)
            .WithModifiers(TokenList(modifiers.Select(Token)))
            .WithLeadingTrivia(
            Trivia(
                DocumentationComment(
                    XmlSummaryElement(
                        XmlText("The "),
                        XmlSeeElement(
                            TypeCref(
                                SyntaxFactory.QualifiedName(context.FullyQualifiedClassName))),
                        XmlText(" builder.")),
                    XmlText(XmlTextNewLine(Constants.NewLine, continueXmlDocumentationComment: false)))))
            .WithMembers([.. CreateMembers(context, builders, useCollectionExpressions)]);

        static IEnumerable<MemberDeclarationSyntax> CreateMembers(BuilderToGenerate context, System.Collections.Immutable.ImmutableArray<BuilderToGenerate> builders, bool useCollectionExpressions)
        {
            foreach (var property in context.Properties)
            {
                if (property.Metadata.HasFlag(PropertyMetadata.ReadOnly))
                {
                    // check to see if the type is a read-only collection
                    if (property.Metadata.HasFlag(PropertyMetadata.Dictionary))
                    {
                        foreach (var member in CreateDictionaryMembers(context.FullyQualifiedClassName, context.BuilderName, property))
                        {
                            yield return member;
                        }

                        continue;
                    }

                    // check to see if the type is a read-only collection
                    if (property.Metadata.HasFlag(PropertyMetadata.Collection))
                    {
                        foreach (var member in CreateCollectionMembers(context.FullyQualifiedClassName, context.BuilderName, property, builders, useCollectionExpressions))
                        {
                            yield return member;
                        }

                        continue;
                    }
                }

                // check to see if the type is a primitive
                if (property.Metadata.HasFlag(PropertyMetadata.Primitive))
                {
                    foreach (var member in CreatePrimitive(context.FullyQualifiedClassName, context.BuilderName, property))
                    {
                        yield return member;
                    }

                    continue;
                }

                foreach (var member in CreateLazy(context.FullyQualifiedClassName, context.BuilderName, property, builders))
                {
                    yield return member;
                }
            }

            var qualifiedClassName = SyntaxFactory.QualifiedName(context.FullyQualifiedClassName);

            // return the build method
            yield return MethodDeclaration(
                    qualifiedClassName,
                    Identifier(BuildMethod))
                .WithModifiers(
                TokenList(
                    Token(SyntaxKind.PublicKeyword)))
                .WithLeadingTrivia(
                Trivia(
                    DocumentationComment(
                        XmlSummaryElement(
                            XmlText("Builds an instance of "),
                            XmlSeeElement(
                                TypeCref(
                                    qualifiedClassName)),
                            XmlText(".")),
                        XmlText(XmlTextNewLine(Constants.NewLine)),
                        XmlReturnsElement(
                            XmlText("The instance of "),
                            XmlSeeElement(
                                TypeCref(
                                    qualifiedClassName)),
                            XmlText(".")),
                        XmlText(XmlTextNewLine(Constants.NewLine, continueXmlDocumentationComment: false)))))
                .WithBody(
                Block(
                    GetBodyStatements(context, builders)));

            static IEnumerable<StatementSyntax> GetBodyStatements(BuilderToGenerate context, System.Collections.Immutable.ImmutableArray<BuilderToGenerate> builders)
            {
                yield return LocalDeclarationStatement(
                        VariableDeclaration(
                            VarIdentifierName)
                        .WithVariables(
                            SingletonSeparatedList(
                                VariableDeclarator(
                                    Identifier(Value))
                                .WithInitializer(
                                    EqualsValueClause(
                                        ObjectCreationExpression(
                                            SyntaxFactory.QualifiedName(context.FullyQualifiedClassName))
                                        .WithArgumentList(
                                            ArgumentList())
                                        .WithInitializer(
                                            InitializerExpression(
                                                SyntaxKind.ObjectInitializerExpression,
                                                SeparatedList(
                                                    GetAssignmentExpressions(context.Properties)))))))));

                foreach (var collection in GetCollectionAssignment(context.Properties, builders))
                {
                    yield return collection;
                }

                foreach (var dictionary in GetDictionaryAssignment(context.Properties))
                {
                    yield return dictionary;
                }

                yield return ReturnStatement(IdentifierName(Value));

                static IEnumerable<ExpressionSyntax> GetAssignmentExpressions(IEnumerable<PropertyToGenerate> properties)
                {
                    return properties
                        .Where(property => !property.Metadata.HasFlag(PropertyMetadata.ReadOnly))
                        .Select(property =>
                            AssignmentExpression(
                                SyntaxKind.SimpleAssignmentExpression,
                                IdentifierName(property.Name),
                                GetMemberAccess(property)));

                    static MemberAccessExpressionSyntax GetMemberAccess(PropertyToGenerate property)
                    {
                        var memberAccess = MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            ThisExpression(),
                            IdentifierName(property.FieldName));
                        if (!property.Metadata.HasFlag(PropertyMetadata.Primitive))
                        {
                            memberAccess = MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                memberAccess,
                                IdentifierName(nameof(Lazy<>.Value)));
                        }

                        return memberAccess;
                    }
                }
            }
        }
    }

    private static SeparatedSyntaxList<TypeSyntax> GetTypeArguments(TypeSyntax typeSyntax)
    {
        if (typeSyntax is QualifiedNameSyntax { Right: GenericNameSyntax { TypeArgumentList.Arguments: { } arguments } })
        {
            return arguments;
        }

        throw new InvalidOperationException();
    }

    private static BlockSyntax GetBuilderActionBlock(BuilderToGenerate builder) =>
        Block(
            LocalDeclarationStatement(
                VariableDeclaration(
                    VarIdentifierName)
                .WithVariables(
                SingletonSeparatedList(
                    VariableDeclarator(
                        Identifier(BuilderVariableName))
                    .WithInitializer(
                    EqualsValueClause(
                        ObjectCreationExpression(
                            SyntaxFactory.QualifiedName(builder.FullQualifiedBuilderName))
                        .WithArgumentList(
                        ArgumentList())))))),
            ExpressionStatement(
                InvocationExpression(
                    IdentifierName(ActionParameterName))
                .WithArgumentList(
                ArgumentList(
                    SingletonSeparatedList(
                        Argument(
                            IdentifierName(BuilderVariableName)))))),
            ReturnStatement(
                InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName(BuilderVariableName),
                        IdentifierName(BuildMethod)))));

    private static ExpressionSyntax CreateExpressionFromTypedConstant(TypedConstant constant)
    {
        return constant switch
        {
            { IsNull: true } => LiteralExpression(SyntaxKind.NullLiteralExpression),
            { Kind: TypedConstantKind.Primitive } => GetLiteralExpression(constant),
            { Kind: TypedConstantKind.Enum } => CreateEnumExpression(constant),
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
                return MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    SyntaxFactory.QualifiedName(enumType.ToString()),
                    IdentifierName(matchingField.Name));
            }

            // If no matching field found, fallback to casting the numeric value
            return CastExpression(
                SyntaxFactory.QualifiedName(enumType.ToString()),
                GetLiteralExpression(constant));
        }

        static LiteralExpressionSyntax GetLiteralExpression(TypedConstant constant)
        {
            return LiteralExpression(
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
                    sbyte i => Literal(i),
                    byte i => Literal(i),
                    short i => Literal(i),
                    ushort i => Literal(i),
                    int i => Literal(i),
                    uint i => Literal(i),
                    long i => Literal(i),
                    ulong i => Literal(i),
                    float f => Literal(f),
                    double f => Literal(f),
                    decimal f => Literal(f),
                    char c => Literal(c),
                    string s => Literal(s),
                    not null => Literal(value.ToString()),
                    _ => Token(SyntaxKind.NullKeyword),
                };
        }
    }
}