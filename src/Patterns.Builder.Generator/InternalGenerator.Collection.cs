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
    private static IEnumerable<MemberDeclarationSyntax> CreateCollectionMembers(string className, string builderName, PropertyToGenerate property, System.Collections.Immutable.ImmutableArray<BuilderToGenerate> builders, bool useCollectionExpressions)
    {
        var suffix = property.Name.Singularize();
        var singularFieldName = property.FieldName.Singularize();
        var typeArgument = GetTypeArguments(property.Type).Single();

        return property.TryGetBuilder(builders, out var builder)
            ? GetBuilderMethods(className, builderName, property, suffix, singularFieldName, typeArgument, builder, useCollectionExpressions)
            : GetBasicMembers(className, builderName, property, suffix, singularFieldName, typeArgument, useCollectionExpressions);

        static SeparatedSyntaxList<ParameterSyntax> GetParameter(TypeSyntax type, string singleName)
        {
            return SingletonSeparatedList<ParameterSyntax>(Parameter(Identifier(singleName)).WithType(type));
        }

        static SeparatedSyntaxList<ArgumentSyntax> GetArguments(string pluralName)
        {
            return SingletonSeparatedList<ArgumentSyntax>(Argument(IdentifierName(pluralName.Singularize())));
        }

        static IEnumerable<MemberDeclarationSyntax> GetBasicMembers(string className, string builderName, PropertyToGenerate property, string suffix, string singularFieldName, TypeSyntax typeArgument, bool useCollectionExpressions)
        {
            ExpressionSyntax collectionCreation = useCollectionExpressions
                ? CollectionExpression()
                : ObjectCreationExpression(typeof(System.Collections.Generic.List<>).ToTypeSyntax(typeArgument)).WithArgumentList(ArgumentList());

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
                                TypeArgumentList(SingletonSeparatedList(typeArgument)))))
                        .WithVariables(
                            SingletonSeparatedList<VariableDeclaratorSyntax>(
                                VariableDeclarator(
                                    Identifier(property.FieldName))
                                .WithInitializer(
                                    EqualsValueClause(
                                        collectionCreation)))));

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
                ParameterList(GetParameter(typeArgument, singularFieldName)))
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
                                GetArguments(singularFieldName)))),
                    ReturnStatement(
                        ThisExpression())));
        }

        static IEnumerable<MemberDeclarationSyntax> GetBuilderMethods(string className, string builderName, PropertyToGenerate property, string suffix, string singularFieldName, TypeSyntax typeArgument, BuilderToGenerate builder, bool useCollectionExpressions)
        {
            ExpressionSyntax collectionCreation = useCollectionExpressions
                ? CollectionExpression()
                : ObjectCreationExpression(typeof(System.Collections.Generic.List<>).ToTypeSyntax(typeArgument)).WithArgumentList(ArgumentList());

            TypeSyntax funcType = typeof(Func<>).ToTypeSyntax(typeArgument);

            yield return FieldDeclaration(
                VariableDeclaration(
                    typeof(System.Collections.Generic.ICollection<>).ToTypeSyntax(funcType))
                .WithVariables(
                    SingletonSeparatedList<VariableDeclaratorSyntax>(
                        VariableDeclarator(
                            Identifier(property.FieldName))
                        .WithInitializer(
                            EqualsValueClause(
                                collectionCreation)))));

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
                ParameterList(GetParameter(typeArgument, singularFieldName)))
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
                                SingletonSeparatedList<ArgumentSyntax>(
                                    Argument(
                                        ParenthesizedLambdaExpression()
                                        .WithExpressionBody(
                                            IdentifierName(singularFieldName))))))),
                    ReturnStatement(
                            ThisExpression())));

            yield return MethodDeclaration(
                IdentifierName(builderName),
                Identifier($"Add{suffix}"))
                .WithModifiers(
                TokenList(
                    Token(property.Accessibility)))
                .WithParameterList(
                ParameterList(
                    GetParameter(funcType, singularFieldName)))
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
                            XmlText(" collection via a factory.")),
                        XmlText(XmlTextNewLine(NewLine)),
                        XmlParamElement(
                            property.FieldName,
                            XmlText($"The {property.FieldName.Humanize(LetterCasing.LowerCase)} factory.")),
                        XmlText(XmlTextNewLine(NewLine)),
                        BuilderReturn,
                        XmlText(XmlTextNewLine(NewLine, continueXmlDocumentationComment: false)))))
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
                                SingletonSeparatedList<ArgumentSyntax>(
                                    Argument(
                                        IdentifierName(singularFieldName)))))),
                    ReturnStatement(
                        ThisExpression())));

            yield return MethodDeclaration(
                IdentifierName(builderName),
                Identifier($"Add{suffix}"))
                .WithModifiers(
                TokenList(
                    Token(property.Accessibility)))
                .WithParameterList(
                ParameterList(
                    SingletonSeparatedList<ParameterSyntax>(
                        Parameter(
                            Identifier(ActionParameterName))
                        .WithType(
                            typeof(Action<>).ToTypeSyntax(NameHelpers.GetQualifiedName(builder.FullQualifiedBuilderName))))))
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
                            XmlText(" collection via a builder.")),
                        XmlText(XmlTextNewLine(NewLine)),
                        XmlParamElement(
                            property.FieldName,
                            XmlText($"The {property.FieldName.Humanize(LetterCasing.LowerCase)} action.")),
                        XmlText(XmlTextNewLine(NewLine)),
                        BuilderReturn,
                        XmlText(XmlTextNewLine(NewLine, continueXmlDocumentationComment: false)))))
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
                                SingletonSeparatedList<ArgumentSyntax>(
                                    Argument(
                                        ParenthesizedLambdaExpression()
                                        .WithBlock(
                                        GetBuilderActionBlock(builder))))))),
                    ReturnStatement(
                        ThisExpression())));
        }
    }

    private static IEnumerable<ForEachStatementSyntax> GetCollectionAssignment(IEnumerable<PropertyToGenerate> properties, IEnumerable<BuilderToGenerate> builders)
    {
        foreach (var property in properties.Where(p => p.ReadOnly && p.Collection && !p.Dictionary))
        {
            ExpressionSyntax expression = IdentifierName(Item);

            // see if this has a builder
            if (property.TryGetBuilder(builders, out _))
            {
                // invoke the function
                expression = InvocationExpression(expression);
            }

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
                    IdentifierName(property.FieldName)),
                Block(
                    SingletonList<StatementSyntax>(
                        ExpressionStatement(
                            InvocationExpression(
                                MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        IdentifierName(Value),
                                        IdentifierName(property.Name)),
                                    IdentifierName(nameof(System.Collections.Generic.ICollection<>.Add))))
                            .WithArgumentList(
                                ArgumentList(
                                    SingletonSeparatedList<ArgumentSyntax>(
                                        Argument(
                                            expression))))))));
        }
    }
}