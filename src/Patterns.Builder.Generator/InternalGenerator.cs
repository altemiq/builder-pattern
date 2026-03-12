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

    private const string NewLine = @"
";

    private const string CreateBuilderMethod = "CreateBuilder";
    private const string BuildMethod = "Build";
    private const string ActionParameterName = "action";
    private const string BuilderVariableName = "builder";

    private static readonly XmlElementSyntax BuilderReturn = XmlReturnsElement(XmlText("The builder for chaining."));

    /// <summary>
    /// Generates the builder.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="builders">The possible builders.</param>
    /// <param name="useCollectionExpressions">Set to <see langword="true"/> to use collection expressions.</param>
    /// <returns>The generated builder.</returns>
    public static CompilationUnitSyntax GenerateNestedBuilder(BuilderToGenerate context, System.Collections.Immutable.ImmutableArray<BuilderToGenerate> builders, bool useCollectionExpressions)
    {
        return InNamespace(
            context.Namespace,
            context.Properties.Any(static property => property.Nullable),
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
                                NameMemberCref(
                                    IdentifierName(context.ClassName))),
                            XmlText(" builder methods."),
                        ])),
                    XmlText(XmlTextNewLine(NewLine, continueXmlDocumentationComment: false)))))
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
                                        NameMemberCref(
                                            IdentifierName(context.ClassName))),
                                    XmlText(" instance.")),
                                XmlText(XmlTextNewLine(NewLine, continueXmlDocumentationComment: false)))))
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
            context.Properties.Any(static property => property.Nullable),
            SingletonList<MemberDeclarationSyntax>(GenerateBuilder(context, [SyntaxKind.SealedKeyword, SyntaxKind.PartialKeyword], builders, useCollectionExpressions)));

    private static CompilationUnitSyntax InNamespace(string @namespace, bool enableNullable, SyntaxList<MemberDeclarationSyntax> members)
    {
        return CompilationUnit()
            .WithMembers(
            SingletonList<MemberDeclarationSyntax>(
                NamespaceDeclaration(NameHelpers.GetQualifiedName(@namespace))
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
                            NameMemberCref(
                                IdentifierName(context.ClassName))),
                        XmlText(" builder.")),
                    XmlText(XmlTextNewLine(NewLine, continueXmlDocumentationComment: false)))))
            .WithMembers([.. CreateMembers(context, builders, useCollectionExpressions)]);

        static IEnumerable<MemberDeclarationSyntax> CreateMembers(BuilderToGenerate context, System.Collections.Immutable.ImmutableArray<BuilderToGenerate> builders, bool useCollectionExpressions)
        {
            foreach (var property in context.Properties)
            {
                if (property.ReadOnly)
                {
                    // check to see if the type is a read-only collection
                    if (property.Dictionary)
                    {
                        foreach (var member in CreateDictionaryMembers(context.ClassName, context.BuilderName, property))
                        {
                            yield return member;
                        }

                        continue;
                    }

                    // check to see if the type is a read-only collection
                    if (property.Collection)
                    {
                        foreach (var member in CreateCollectionMembers(context.ClassName, context.BuilderName, property, builders, useCollectionExpressions))
                        {
                            yield return member;
                        }

                        continue;
                    }
                }

                // check to see if the type is a primitive
                if (property.Primitive)
                {
                    foreach (var member in CreatePrimitive(context.ClassName, context.BuilderName, property))
                    {
                        yield return member;
                    }

                    continue;
                }

                foreach (var member in CreateLazy(context.ClassName, context.BuilderName, property, builders))
                {
                    yield return member;
                }
            }

            // return the build method
            yield return MethodDeclaration(
                IdentifierName(context.ClassName),
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
                                NameMemberCref(
                                    IdentifierName(context.ClassName))),
                            XmlText(".")),
                        XmlText(XmlTextNewLine(NewLine)),
                        XmlReturnsElement(
                            XmlText("The instance of "),
                            XmlSeeElement(
                                NameMemberCref(
                                    IdentifierName(context.ClassName))),
                            XmlText(".")),
                        XmlText(XmlTextNewLine(NewLine, continueXmlDocumentationComment: false)))))
                .WithBody(
                Block(
                    GetBodyStatements(context, builders)));

            static IEnumerable<StatementSyntax> GetBodyStatements(BuilderToGenerate context, System.Collections.Immutable.ImmutableArray<BuilderToGenerate> builders)
            {
                yield return LocalDeclarationStatement(
                        VariableDeclaration(
                            IdentifierName(
                                Identifier(
                                    TriviaList(),
                                    SyntaxKind.VarKeyword,
                                    "var",
                                    "var",
                                    TriviaList())))
                        .WithVariables(
                            SingletonSeparatedList<VariableDeclaratorSyntax>(
                                VariableDeclarator(
                                    Identifier(Value))
                                .WithInitializer(
                                    EqualsValueClause(
                                        ObjectCreationExpression(
                                            IdentifierName(context.ClassName))
                                        .WithArgumentList(
                                            ArgumentList())
                                        .WithInitializer(
                                            InitializerExpression(
                                                SyntaxKind.ObjectInitializerExpression,
                                                SeparatedList<ExpressionSyntax>(
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
                        .Where(property => !property.ReadOnly)
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
                        if (!property.Primitive)
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
                    IdentifierName(
                        Identifier(
                            TriviaList(),
                            SyntaxKind.VarKeyword,
                            "var",
                            "var",
                            TriviaList())))
                .WithVariables(
                SingletonSeparatedList<VariableDeclaratorSyntax>(
                    VariableDeclarator(
                        Identifier(BuilderVariableName))
                    .WithInitializer(
                    EqualsValueClause(
                        ObjectCreationExpression(
                            NameHelpers.GetQualifiedName(builder.FullQualifiedBuilderName))
                        .WithArgumentList(
                        ArgumentList())))))),
            ExpressionStatement(
                InvocationExpression(
                    IdentifierName(ActionParameterName))
                .WithArgumentList(
                ArgumentList(
                    SingletonSeparatedList<ArgumentSyntax>(
                        Argument(
                            IdentifierName(BuilderVariableName)))))),
            ReturnStatement(
                InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName(BuilderVariableName),
                        IdentifierName(BuildMethod)))));
}