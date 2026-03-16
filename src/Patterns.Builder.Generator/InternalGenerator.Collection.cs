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
    private static IEnumerable<MemberDeclarationSyntax> CreateCollectionMembers(
        string className,
        string builderName,
        PropertyToGenerate property,
        ICollection<BuilderToGenerate> builders,
        bool useCollectionExpressions)
    {
        var suffix = property.Name.Singularize();
        var singularFieldName = property.FieldName.Singularize();
        var typeArgument = GetTypeArguments(property.Type).Single();

        return property.TryGetBuilder(builders, out var builder)
            ? GetBuilderMethods(className, builderName, property, suffix, singularFieldName, typeArgument, builder, useCollectionExpressions)
            : GetBasicMembers(className, builderName, property, suffix, singularFieldName, typeArgument, useCollectionExpressions);

        static SeparatedSyntaxList<ParameterSyntax> GetParameter(TypeSyntax type, string singleName)
        {
            return SingletonSeparatedList(Parameter(Identifier(singleName)).WithType(type));
        }

        static SeparatedSyntaxList<ArgumentSyntax> GetArguments(string pluralName)
        {
            return SingletonSeparatedList(Argument(IdentifierName(pluralName.Singularize())));
        }

        static IEnumerable<MemberDeclarationSyntax> GetBasicMembers(string className, string builderName, PropertyToGenerate property, string suffix, string singularFieldName, TypeSyntax typeArgument, bool useCollectionExpressions)
        {
            ExpressionSyntax collectionCreation = useCollectionExpressions
                ? CollectionExpression()
                : ObjectCreationExpression(typeof(List<>).ToTypeSyntax([typeArgument])).WithArgumentList(ArgumentList());

            yield return FieldDeclaration(
                VariableDeclaration(
                        typeof(ICollection<>).ToTypeSyntax([typeArgument]))
                    .WithVariables(
                        SingletonSeparatedList(
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
                            XmlText(XmlTextNewLine(Constants.NewLine)),
                            XmlParamElement(
                                singularFieldName,
                                XmlText($"The {singularFieldName.Humanize(LetterCasing.LowerCase)} to add.")),
                            XmlText(XmlTextNewLine(Constants.NewLine)),
                            BuilderReturn,
                            XmlText(XmlTextNewLine(Constants.NewLine, continueXmlDocumentationComment: false)))))
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

        static IEnumerable<MemberDeclarationSyntax> GetBuilderMethods(
            string className,
            string builderName,
            PropertyToGenerate property,
            string suffix,
            string singularFieldName,
            TypeSyntax typeArgument,
            BuilderToGenerate builder,
            bool useCollectionExpressions)
        {
            ExpressionSyntax collectionCreation = useCollectionExpressions
                ? CollectionExpression()
                : ObjectCreationExpression(typeof(List<>).ToTypeSyntax([typeArgument])).WithArgumentList(ArgumentList());

            TypeSyntax funcType = typeof(Func<>).ToTypeSyntax([typeArgument]);

            yield return FieldDeclaration(
                VariableDeclaration(
                        typeof(ICollection<>).ToTypeSyntax([funcType]))
                    .WithVariables(
                        SingletonSeparatedList(
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
                            XmlText(XmlTextNewLine(Constants.NewLine)),
                            XmlParamElement(
                                singularFieldName,
                                XmlText($"The {singularFieldName.Humanize(LetterCasing.LowerCase)} to add.")),
                            XmlText(XmlTextNewLine(Constants.NewLine)),
                            BuilderReturn,
                            XmlText(XmlTextNewLine(Constants.NewLine, continueXmlDocumentationComment: false)))))
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
                                        SingletonSeparatedList(
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
                            XmlText(XmlTextNewLine(Constants.NewLine)),
                            XmlParamElement(
                                property.FieldName,
                                XmlText($"The {property.FieldName.Humanize(LetterCasing.LowerCase)} factory.")),
                            XmlText(XmlTextNewLine(Constants.NewLine)),
                            BuilderReturn,
                            XmlText(XmlTextNewLine(Constants.NewLine, continueXmlDocumentationComment: false)))))
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
                                        SingletonSeparatedList(
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
                        SingletonSeparatedList(
                            Parameter(
                                    Identifier(ActionParameterName))
                                .WithType(
                                    typeof(Action<>).ToTypeSyntax([NameHelpers.GetQualifiedName(builder.FullQualifiedBuilderName)])))))
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
                            XmlText(XmlTextNewLine(Constants.NewLine)),
                            XmlParamElement(
                                property.FieldName,
                                XmlText($"The {property.FieldName.Humanize(LetterCasing.LowerCase)} action.")),
                            XmlText(XmlTextNewLine(Constants.NewLine)),
                            BuilderReturn,
                            XmlText(XmlTextNewLine(Constants.NewLine, continueXmlDocumentationComment: false)))))
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
                                        SingletonSeparatedList(
                                            Argument(
                                                ParenthesizedLambdaExpression()
                                                    .WithBlock(
                                                        GetBuilderActionBlock(builder))))))),
                        ReturnStatement(
                            ThisExpression())));
        }
    }

    private static IEnumerable<ForEachStatementSyntax> GetCollectionAssignment(IEnumerable<PropertyToGenerate> properties, ICollection<BuilderToGenerate> builders)
    {
        foreach (var property in properties.Where(p => p.Metadata.HasFlag(PropertyMetadata.ReadOnly) && p.Metadata.HasFlag(PropertyMetadata.Collection) && !p.Metadata.HasFlag(PropertyMetadata.Dictionary)))
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
                                        IdentifierName(nameof(ICollection<>.Add))))
                                .WithArgumentList(
                                    ArgumentList(
                                        SingletonSeparatedList(
                                            Argument(
                                                expression))))))));
        }
    }
}