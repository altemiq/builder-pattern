// -----------------------------------------------------------------------
// <copyright file="InternalGenerator.Dictionary.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.Patterns.Builder.Generator;

using Humanizer;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

/// <content>
/// The <see cref="IDictionary{TKey, TValue}"/> generators.
/// </content>
internal static partial class InternalGenerator
{
    private static IEnumerable<MemberDeclarationSyntax> CreateDictionaryMembers(string className, string builderName, PropertyToGenerate property)
    {
        var suffix = property.Name.Singularize();
        var typeArguments = GetTypeArguments(property.Type);
        var genericCollectionNamespace = QualifiedName(
            QualifiedName(
                IdentifierName(nameof(System)),
                IdentifierName(nameof(System.Collections))),
            IdentifierName(nameof(System.Collections.Generic)));
        var keyValuePairName = QualifiedName(
            genericCollectionNamespace,
            GenericName(
                Identifier(nameof(System.Collections.Generic.KeyValuePair<,>)))
            .WithTypeArgumentList(
                TypeArgumentList(
                    typeArguments)));

        yield return FieldDeclaration(
            VariableDeclaration(
                QualifiedName(
                    genericCollectionNamespace,
                    GenericName(
                        Identifier(nameof(System.Collections.Generic.ICollection<>)))
                    .WithTypeArgumentList(
                        TypeArgumentList(
                            SingletonSeparatedList<TypeSyntax>(
                                keyValuePairName)))))
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
                        XmlText("Adds a key/value pair to the "),
                        XmlSeeElement(
                            QualifiedCref(
                                IdentifierName(className),
                                NameMemberCref(
                                    IdentifierName(property.Name)))),
                        XmlText(" dictionary.")),
                    XmlText(XmlTextNewLine(NewLine)),
                    XmlParamElement(
                        Key,
                        XmlText($"The {property.FieldName.Humanize(LetterCasing.LowerCase)} key.")),
                    XmlText(XmlTextNewLine(NewLine)),
                    XmlParamElement(
                        Value,
                        XmlText($"The {property.FieldName.Humanize(LetterCasing.LowerCase)} value.")),
                    XmlText(XmlTextNewLine(NewLine)),
                    BuilderReturn,
                    XmlText(XmlTextNewLine(NewLine, continueXmlDocumentationComment: false)))))
            .WithParameterList(
                ParameterList(GetParameters(typeArguments)))
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
                                GetArguments(typeArguments, keyValuePairName)))),
                    ReturnStatement(
                        ThisExpression())));

        static SeparatedSyntaxList<ParameterSyntax> GetParameters(SeparatedSyntaxList<TypeSyntax> types)
        {
            if (types is [var keyType, var valueType])
            {
                // return as key/value pair
                return SeparatedList<ParameterSyntax>(
                [
                    Parameter(Identifier(Key)).WithType(keyType),
                    Parameter(Identifier(Value)).WithType(valueType),
                ]);
            }

            throw new NotSupportedException();
        }

        static SeparatedSyntaxList<ArgumentSyntax> GetArguments(SeparatedSyntaxList<TypeSyntax> types, TypeSyntax keyValuePair)
        {
            if (types.Count is 2)
            {
                // return as key/value pair
                return SingletonSeparatedList<ArgumentSyntax>(
                    Argument(
                        ObjectCreationExpression(keyValuePair)
                        .WithArgumentList(
                            ArgumentList(
                                SeparatedList<ArgumentSyntax>(
                                [
                                    Argument(IdentifierName(Key)),
                                    Argument(IdentifierName(Value)),
                                ])))));
            }

            throw new NotSupportedException();
        }
    }

    private static IEnumerable<ForEachStatementSyntax> GetDictionaryAssignment(IEnumerable<PropertyToGenerate> properties)
    {
        foreach (var p in properties.Where(p => p.ReadOnly && p.Collection && p.Dictionary))
        {
            yield return ForEachStatement(
                IdentifierName(
                    Identifier(
                        TriviaList(),
                        SyntaxKind.VarKeyword,
                        "var",
                        "var",
                        TriviaList())),
                Identifier(KeyValuePair),
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
                                    IdentifierName(nameof(System.Collections.Generic.IDictionary<,>.Add))))
                            .WithArgumentList(
                                ArgumentList(
                                    SeparatedList<ArgumentSyntax>(
                                    [
                                            Argument(
                                                MemberAccessExpression(
                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                    IdentifierName(KeyValuePair),
                                                    IdentifierName(nameof(KeyValuePair<,>.Key)))),
                                        Argument(
                                            MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                IdentifierName(KeyValuePair),
                                                IdentifierName(nameof(KeyValuePair<,>.Value)))),
                                    ])))))));
        }
    }
}