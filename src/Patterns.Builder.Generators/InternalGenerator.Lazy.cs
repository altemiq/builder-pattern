// -----------------------------------------------------------------------
// <copyright file="InternalGenerator.Lazy.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.Patterns.Builder.Generators;

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
        var qualifiedClassName = SyntaxFactory.QualifiedName(className);
        var lazyType = typeof(Lazy<>).ToTypeSyntax([property.Type]);
        var funcType = typeof(Func<>).ToTypeSyntax([property.Type]);

        var defaultValueExpression = property.DefaultValue switch
        {
            LambdaExpressionSyntax lambdaDefaultExpression => lambdaDefaultExpression,
            { } defaultExpression => ParenthesizedLambdaExpression().WithExpressionBody(defaultExpression),
            _ when property.Metadata.HasFlag(PropertyMetadata.Nullable) => ParenthesizedLambdaExpression()
                .WithExpressionBody(
                    PostfixUnaryExpression(
                        SyntaxKind.SuppressNullableWarningExpression,
                        LiteralExpression(
                            SyntaxKind.DefaultLiteralExpression,
                            Token(SyntaxKind.DefaultKeyword)))),
            _ => ParenthesizedLambdaExpression()
                .WithExpressionBody(
                    LiteralExpression(
                        SyntaxKind.DefaultLiteralExpression,
                        Token(SyntaxKind.DefaultKeyword))),
        };

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
                                                            defaultValueExpression)))))))))
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
                                    qualifiedClassName,
                                    NameMemberCref(
                                        IdentifierName(property.Name)))),
                            XmlText(" value.")),
                        XmlText(XmlTextNewLine(Constants.NewLine)),
                        XmlParamElement(
                            property.FieldName.TrimStart('@'),
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
                                    SyntaxFactory.QualifiedName(className),
                                    NameMemberCref(
                                        IdentifierName(property.Name)))),
                            XmlText(" value via a factory.")),
                        XmlText(XmlTextNewLine(Constants.NewLine)),
                        XmlParamElement(
                            property.FieldName.TrimStart('@'),
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

        // do any constructors
        foreach (var constructor in property.Constructors)
        {
            // if this has any ref-like parameters we can't use it
            if (constructor.Parameters.Any(p => p.Type.IsRefLikeType))
            {
                continue;
            }

            var documentation = new List<XmlNodeSyntax>
            {
                XmlSummaryElement(
                    XmlText("Sets the "),
                    XmlSeeElement(
                        QualifiedCref(
                            qualifiedClassName,
                            NameMemberCref(
                                IdentifierName(property.Name)))),
                    XmlText(" value via a constructor.")),
                XmlText(XmlTextNewLine(Constants.NewLine)),
            };

            if (constructor.GetDocumentationCommentXml() is { Length: > 0 } xml
                && System.Xml.Linq.XDocument.Parse(xml) is { Root: { } root })
            {
                // get the parameters
                foreach (var node in root.Nodes().OfType<System.Xml.Linq.XElement>().Where(n => n.Name.LocalName is "param"))
                {
                    documentation.Add(node.ToXmlNode());
                    documentation.Add(XmlText(XmlTextNewLine(Constants.NewLine)));
                }
            }

            documentation.Add(BuilderReturn);
            documentation.Add(XmlText(XmlTextNewLine(Constants.NewLine, continueXmlDocumentationComment: false)));

            yield return MethodDeclaration(
                    IdentifierName(builderName),
                    Identifier($"With{property.Name}"))
                .WithModifiers(
                    TokenList(
                        Token(property.Accessibility)))
                .WithLeadingTrivia(
                    Trivia(
                        DocumentationComment(
                            [.. documentation])))
                .WithParameterList(
                    constructor.GetParameterList())
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
                                                            constructor.GetObjectCreation()))))))),
                        ReturnStatement(
                            ThisExpression())));
        }

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
                                        qualifiedClassName,
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
                                    typeof(Action<>).ToTypeSyntax([
                                        SyntaxFactory.QualifiedName(builder.FullQualifiedBuilderName),
                                    ])))))
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