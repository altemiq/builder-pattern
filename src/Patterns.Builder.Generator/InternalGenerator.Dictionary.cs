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
        var keyValuePairName = typeof(KeyValuePair<,>).ToTypeSyntax(typeArguments);

        yield return FieldDeclaration(
            VariableDeclaration(
                    typeof(ICollection<>).ToTypeSyntax([keyValuePairName]))
                .WithVariables(
                    SingletonSeparatedList(
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
                        XmlText(XmlTextNewLine(Constants.NewLine)),
                        XmlParamElement(
                            Key,
                            XmlText($"The {property.FieldName.Humanize(LetterCasing.LowerCase)} key.")),
                        XmlText(XmlTextNewLine(Constants.NewLine)),
                        XmlParamElement(
                            Value,
                            XmlText($"The {property.FieldName.Humanize(LetterCasing.LowerCase)} value.")),
                        XmlText(XmlTextNewLine(Constants.NewLine)),
                        BuilderReturn,
                        XmlText(XmlTextNewLine(Constants.NewLine, continueXmlDocumentationComment: false)))))
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
                return SeparatedList(
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
                return SingletonSeparatedList(
                    Argument(
                        ObjectCreationExpression(keyValuePair)
                            .WithArgumentList(
                                ArgumentList(
                                    SeparatedList(
                                    [
                                        Argument(IdentifierName(Key)),
                                        Argument(IdentifierName(Value)),
                                    ])))));
            }

            throw new NotSupportedException();
        }
    }

    private static IEnumerable<ForEachStatementSyntax> GetDictionaryAssignment(IEnumerable<PropertyToGenerate> properties) =>
        properties
            .Where(p => p.Metadata.HasFlag(PropertyMetadata.ReadOnly) && p.Metadata.HasFlag(PropertyMetadata.Collection) && p.Metadata.HasFlag(PropertyMetadata.Dictionary))
            .Select(p =>
                ForEachStatement(
                    VarIdentifierName,
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
                                            IdentifierName(nameof(IDictionary<,>.Add))))
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
                                            ]))))))));
}