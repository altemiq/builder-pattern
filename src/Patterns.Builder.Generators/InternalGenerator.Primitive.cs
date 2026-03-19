// -----------------------------------------------------------------------
// <copyright file="InternalGenerator.Primitive.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.Patterns.Builder.Generators;

using Humanizer;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

/// <content>
/// The primitive generators.
/// </content>
internal static partial class InternalGenerator
{
    private static IEnumerable<MemberDeclarationSyntax> CreatePrimitive(string className, string builderName, PropertyToGenerate property)
    {
        var variableDeclarator = VariableDeclarator(
            Identifier(property.FieldName));

        if (property.DefaultValue is { } defaultValue)
        {
            variableDeclarator = variableDeclarator
                .WithInitializer(
                    EqualsValueClause(defaultValue));
        }

        yield return FieldDeclaration(
                VariableDeclaration(property.Type)
                    .WithVariables(
                        SingletonSeparatedList(
                            variableDeclarator)))
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
                                    SyntaxFactory.QualifiedName(className),
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
                            IdentifierName(property.FieldName))),
                    ReturnStatement(
                        ThisExpression())));
    }
}