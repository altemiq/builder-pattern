// -----------------------------------------------------------------------
// <copyright file="InternalGenerator.Lazy.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.Patterns.Builder.Generator;

using Humanizer;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

/// <content>
/// The <see cref="Lazy{T}"/> generators.
/// </content>
internal static partial class InternalGenerator
{
    private static IEnumerable<MemberDeclarationSyntax> CreateLazy(string className, string builderName, PropertyToGenerate property, System.Collections.Immutable.ImmutableArray<BuilderToGenerate> builders)
    {
        var lazyType = typeof(Lazy<>).ToTypeSyntax(property.Type);

        yield return FieldDeclaration(
            VariableDeclaration(lazyType)
            .WithVariables(
                SingletonSeparatedList<VariableDeclaratorSyntax>(
                    VariableDeclarator(
                        Identifier(property.FieldName)))))
            .WithModifiers(
                TokenList(
                    Token(SyntaxKind.PrivateKeyword)));

        yield return MethodDeclaration(
            IdentifierName(builderName),
            Identifier($"With{property.Name}"))
            .WithModifiers(
            TokenList(
                Token(property.Accessibility)))
            .WithLeadingTrivia(
            Trivia(
                DocumentationComment(
                    XmlSummaryElement(
                        XmlText("Sets the "),
                        XmlSeeElement(
                            QualifiedCref(
                                IdentifierName(className),
                                NameMemberCref(
                                    IdentifierName(property.Name)))),
                        XmlText(" value.")),
                    XmlText(XmlTextNewLine(NewLine)),
                    XmlParamElement(
                        property.FieldName,
                        XmlText($"The {property.FieldName.Humanize(LetterCasing.LowerCase)} value.")),
                    XmlText(XmlTextNewLine(NewLine)),
                    BuilderReturn,
                    XmlText(XmlTextNewLine(NewLine, continueXmlDocumentationComment: false)))))
            .WithParameterList(
            ParameterList(
                SingletonSeparatedList<ParameterSyntax>(
                    Parameter(Identifier(property.FieldName))
                    .WithType(property.Type))))
            .WithBody(
            Block(
                ExpressionStatement(
                    AssignmentExpression(
                        SyntaxKind.SimpleAssignmentExpression,
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            ThisExpression(),
                            IdentifierName(property.FieldName)),
                        ObjectCreationExpression(
                            lazyType)
                        .WithArgumentList(
                            ArgumentList(
                                SingletonSeparatedList<ArgumentSyntax>(
                                    Argument(
                                        ParenthesizedLambdaExpression()
                                        .WithExpressionBody(
                                            IdentifierName(property.FieldName)))))))),
                ReturnStatement(
                    ThisExpression())));

        yield return MethodDeclaration(
            IdentifierName(builderName),
            Identifier($"With{property.Name}"))
            .WithModifiers(
            TokenList(
                Token(property.Accessibility)))
            .WithLeadingTrivia(
            Trivia(
                DocumentationComment(
                    XmlSummaryElement(
                        XmlText("Sets the "),
                        XmlSeeElement(
                            QualifiedCref(
                                IdentifierName(className),
                                NameMemberCref(
                                    IdentifierName(property.Name)))),
                        XmlText(" value via a factory.")),
                    XmlText(XmlTextNewLine(NewLine)),
                    XmlParamElement(
                        property.FieldName,
                        XmlText($"The {property.FieldName.Humanize(LetterCasing.LowerCase)} factory.")),
                    XmlText(XmlTextNewLine(NewLine)),
                    BuilderReturn,
                    XmlText(XmlTextNewLine(NewLine, continueXmlDocumentationComment: false)))))
            .WithParameterList(
            ParameterList(
                SingletonSeparatedList<ParameterSyntax>(
                    Parameter(
                        Identifier(property.FieldName))
                    .WithType(
                        lazyType))))
            .WithBody(
            Block(
                ExpressionStatement(
                    AssignmentExpression(
                        SyntaxKind.SimpleAssignmentExpression,
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            ThisExpression(),
                            IdentifierName(property.FieldName)),
                        IdentifierName(property.FieldName))),
                ReturnStatement(
                    ThisExpression())));

        // check to see if the type of class is one of the builders
        if (builders.FirstOrDefault(b => string.Equals(b.FullyQualifiedClassName, property.Type.ToFullString(), StringComparison.Ordinal)) is { ClassName: not null } builder)
        {
            const string ActionParameterName = "action";
            const string BuilderVariableName = "builder";

            // create a method that takes the builder
            yield return MethodDeclaration(
                IdentifierName(builderName),
                Identifier($"With{property.Name}"))
                .WithModifiers(
                TokenList(
                    Token(property.Accessibility)))
                .WithLeadingTrivia(
                Trivia(
                    DocumentationComment(
                        XmlSummaryElement(
                            XmlText("Sets the "),
                            XmlSeeElement(
                                QualifiedCref(
                                    IdentifierName(className),
                                    NameMemberCref(
                                        IdentifierName(property.Name)))),
                            XmlText(" value via a builder.")),
                        XmlText(XmlTextNewLine(NewLine)),
                        XmlParamElement(
                            ActionParameterName,
                            XmlText($"The {property.FieldName.Humanize(LetterCasing.LowerCase)} action.")),
                        XmlText(XmlTextNewLine(NewLine)),
                        BuilderReturn,
                        XmlText(XmlTextNewLine(NewLine, continueXmlDocumentationComment: false)))))
                .WithParameterList(
                ParameterList(
                    SingletonSeparatedList<ParameterSyntax>(
                        Parameter(
                            Identifier(ActionParameterName))
                        .WithType(
                            typeof(System.Action<>).ToTypeSyntax(NameHelpers.GetQualifiedName(builder.FullQualifiedBuilderName))))))
                .WithBody(
                Block(
                    ExpressionStatement(
                        AssignmentExpression(
                            SyntaxKind.SimpleAssignmentExpression,
                            MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                ThisExpression(),
                                IdentifierName(property.FieldName)),
                            ObjectCreationExpression(lazyType)
                            .WithArgumentList(
                                ArgumentList(
                                    SingletonSeparatedList<ArgumentSyntax>(
                                        Argument(
                                            ParenthesizedLambdaExpression()
                                            .WithBlock(
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
                                                                IdentifierName(BuildMethod)))))))))))),
                    ReturnStatement(
                        ThisExpression())));
                }
    }
}