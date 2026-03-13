// -----------------------------------------------------------------------
// <copyright file="InternalGenerator.Primitive.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.Patterns.Builder.Generator;

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
        yield return FieldDeclaration(
            VariableDeclaration(property.Type)
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
                    XmlText(XmlTextNewLine(Constants.NewLine)),
                    XmlParamElement(
                        property.FieldName,
                        XmlText($"The {property.FieldName.Humanize(LetterCasing.LowerCase)} value.")),
                    XmlText(XmlTextNewLine(Constants.NewLine)),
                    BuilderReturn,
                    XmlText(XmlTextNewLine(Constants.NewLine, continueXmlDocumentationComment: false)))))
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
                        IdentifierName(property.FieldName))),
                ReturnStatement(
                    ThisExpression())));
    }
}