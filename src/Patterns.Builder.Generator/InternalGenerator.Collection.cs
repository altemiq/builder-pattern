// -----------------------------------------------------------------------
// <copyright file="InternalGenerator.Collection.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.Patterns.Builder.Generator;

using Humanizer;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

/// <content>
/// The <see cref="ICollection{T}"/> generators.
/// </content>
internal static partial class InternalGenerator
{
    private static IEnumerable<MemberDeclarationSyntax> CreateCollectionMembers(string className, string builderName, PropertyToGenerate property)
    {
        var suffix = property.Name.Singularize();
        var singularFieldName = property.FieldName.Singularize();
        var typeArguments = GetTypeArguments(property.Type);

        yield return FieldDeclaration(
            VariableDeclaration(
                QualifiedName(
                    QualifiedName(
                        QualifiedName(
                            IdentifierName(nameof(System)),
                            IdentifierName(nameof(System.Collections))),
                        IdentifierName(nameof(System.Collections.Generic))),
                    GenericName(
                        Identifier(nameof(System.Collections.Generic.ICollection<>)))
                    .WithTypeArgumentList(
                        TypeArgumentList(typeArguments))))
            .WithVariables(
                SingletonSeparatedList<VariableDeclaratorSyntax>(
                    VariableDeclarator(
                        Identifier(property.FieldName))
                    .WithInitializer(
                        EqualsValueClause(
                            CollectionExpression())))));

        yield return MethodDeclaration(
                IdentifierName(builderName),
                Identifier($"Add{suffix}"))
            .WithModifiers(
                TokenList(
                    Token(property.Accessibility)))
            .WithLeadingTrivia(
            Trivia(
                DocumentationComment(
                    XmlSummaryElement(
                        XmlText("Adds a value to the "),
                        XmlSeeElement(
                            QualifiedCref(
                                IdentifierName(className),
                                NameMemberCref(
                                    IdentifierName(property.Name)))),
                        XmlText(" collection.")),
                    XmlText(XmlTextNewLine(NewLine)),
                    XmlParamElement(
                        singularFieldName,
                        XmlText($"The {singularFieldName.Humanize(LetterCasing.LowerCase)} to add.")),
                    XmlText(XmlTextNewLine(NewLine)),
                    BuilderReturn,
                    XmlText(XmlTextNewLine(NewLine, continueXmlDocumentationComment: false)))))
            .WithParameterList(
                ParameterList(GetParameters(typeArguments, singularFieldName)))
            .WithBody(
                Block(
                    ExpressionStatement(
                        InvocationExpression(
                            MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    ThisExpression(),
                                    IdentifierName(property.FieldName)),
                                IdentifierName(nameof(ICollection<>.Add))))
                        .WithArgumentList(
                            ArgumentList(
                                GetArguments(typeArguments, property.FieldName)))),
                    ReturnStatement(
                        ThisExpression())));

        static SeparatedSyntaxList<ParameterSyntax> GetParameters(SeparatedSyntaxList<TypeSyntax> types, string singleName)
        {
            if (types is [var type])
            {
                // return the single type, called value
                return SingletonSeparatedList<ParameterSyntax>(Parameter(Identifier(singleName)).WithType(type));
            }

            throw new NotSupportedException();
        }

        static SeparatedSyntaxList<ArgumentSyntax> GetArguments(SeparatedSyntaxList<TypeSyntax> types, string pluralName)
        {
            if (types.Count is 1)
            {
                // return the single type, called value
                return SingletonSeparatedList<ArgumentSyntax>(Argument(IdentifierName(pluralName.Singularize())));
            }

            throw new NotSupportedException();
        }
    }

    private static IEnumerable<ForEachStatementSyntax> GetCollectionAssignment(IEnumerable<PropertyToGenerate> properties)
    {
        foreach (var p in properties.Where(p => p.ReadOnly && p.Collection && !p.Dictionary))
        {
            yield return ForEachStatement(
                IdentifierName(
                    Identifier(
                        TriviaList(),
                        SyntaxKind.VarKeyword,
                        "var",
                        "var",
                        TriviaList())),
                Identifier(Item),
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    ThisExpression(),
                    IdentifierName(p.FieldName)),
                Block(
                    SingletonList<StatementSyntax>(
                        ExpressionStatement(
                            InvocationExpression(
                                MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        IdentifierName(Value),
                                        IdentifierName(p.Name)),
                                    IdentifierName(nameof(System.Collections.Generic.ICollection<>.Add))))
                            .WithArgumentList(
                                ArgumentList(
                                    SingletonSeparatedList<ArgumentSyntax>(
                                        Argument(
                                            IdentifierName(Item)))))))));
        }
    }
}