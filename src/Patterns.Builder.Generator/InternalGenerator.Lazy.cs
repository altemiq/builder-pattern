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
        var lazyType = typeof(Lazy<>).ToTypeSyntax([property.Type]);
        var funcType = typeof(Func<>).ToTypeSyntax([property.Type]);

        yield return FieldDeclaration(
            VariableDeclaration(lazyType)
            .WithVariables(
                SingletonSeparatedList(
                    VariableDeclarator(
                        Identifier(property.FieldName))
                    .WithInitializer(
                    EqualsValueClause(
                        ObjectCreationExpression(lazyType)
                        .WithArgumentList(
                        ArgumentList(
                            SingletonSeparatedList(
                                Argument(
                                    ParenthesizedLambdaExpression()
                                    .WithExpressionBody(
                                    LiteralExpression(
                                        SyntaxKind.DefaultLiteralExpression,
                                        Token(SyntaxKind.DefaultKeyword))))))))))))
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
                    XmlText(XmlTextNewLine(Constants.NewLine)),
                    XmlParamElement(
                        property.FieldName,
                        XmlText($"The {property.FieldName.Humanize(LetterCasing.LowerCase)} value.")),
                    XmlText(XmlTextNewLine(Constants.NewLine)),
                    BuilderReturn,
                    XmlText(XmlTextNewLine(Constants.NewLine, continueXmlDocumentationComment: false)))))
            .WithParameterList(
            ParameterList(
                SingletonSeparatedList(
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
                                SingletonSeparatedList(
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
                    XmlText(XmlTextNewLine(Constants.NewLine)),
                    XmlParamElement(
                        property.FieldName,
                        XmlText($"The {property.FieldName.Humanize(LetterCasing.LowerCase)} factory.")),
                    XmlText(XmlTextNewLine(Constants.NewLine)),
                    BuilderReturn,
                    XmlText(XmlTextNewLine(Constants.NewLine, continueXmlDocumentationComment: false)))))
            .WithParameterList(
            ParameterList(
                SingletonSeparatedList(
                    Parameter(
                        Identifier(property.FieldName))
                    .WithType(
                        funcType))))
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
                                SingletonSeparatedList(
                                    Argument(IdentifierName(property.FieldName))))))),
                ReturnStatement(
                    ThisExpression())));

        // check to see if the type of class is one of the builders
        if (property.TryGetBuilder(builders, out var builder))
        {
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
                        XmlText(XmlTextNewLine(Constants.NewLine)),
                        XmlParamElement(
                            ActionParameterName,
                            XmlText($"The {property.FieldName.Humanize(LetterCasing.LowerCase)} action.")),
                        XmlText(XmlTextNewLine(Constants.NewLine)),
                        BuilderReturn,
                        XmlText(XmlTextNewLine(Constants.NewLine, continueXmlDocumentationComment: false)))))
                .WithParameterList(
                ParameterList(
                    SingletonSeparatedList(
                        Parameter(
                            Identifier(ActionParameterName))
                        .WithType(
                            typeof(Action<>).ToTypeSyntax([NameHelpers.GetQualifiedName(builder.FullQualifiedBuilderName)])))))
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
                                    SingletonSeparatedList(
                                        Argument(
                                            ParenthesizedLambdaExpression()
                                            .WithBlock(
                                            GetBuilderActionBlock(builder)))))))),
                    ReturnStatement(
                        ThisExpression())));
        }
    }
}