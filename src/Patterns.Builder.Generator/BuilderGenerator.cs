// -----------------------------------------------------------------------
// <copyright file="BuilderGenerator.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.Patterns.Builder.Generator;

using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

/// <summary>
/// The <see cref="Builder"/> <see cref="IIncrementalGenerator"/>.
/// </summary>
[Generator(LanguageNames.CSharp)]
public class BuilderGenerator : IIncrementalGenerator
{
    /// <inheritdoc/>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var compilationDetails = context.CompilationProvider
            .Select((x, _) => x is CSharpCompilation compilation ? compilation.LanguageVersion : default);

        // get anything marked by the marker
        var buildersToGenerate =
            context
                .SyntaxProvider
                .ForAttributeWithMetadataName<BuilderToGenerate?>(
                    TypeNames.MarkerAttribute,
                    predicate: (node, _) => node is ClassDeclarationSyntax or StructDeclarationSyntax,
                    transform: GetNestedTypeToGenerate)
                .WithTrackingName(TrackingNames.InitialExtraction)
                .Where(static b => b is not null)
                .Select(static (b, _) => b!.Value)
                .WithTrackingName(TrackingNames.RemovingNulls);

        var allBuilders = buildersToGenerate.Collect();

        context.RegisterSourceOutput(
            buildersToGenerate.Combine(allBuilders).Combine(compilationDetails),
            (context, source) => Execute(source.Left.Left, source.Left.Right, source.Right, context));

        static BuilderToGenerate? GetNestedTypeToGenerate(GeneratorAttributeSyntaxContext context, CancellationToken cancellationToken)
        {
            if (context.TargetSymbol is not ITypeSymbol typeSymbol)
            {
                return null;
            }

            cancellationToken.ThrowIfCancellationRequested();

            var collectionTypeSymbol = context.SemanticModel.Compilation.GetTypeByMetadataName("System.Collections.Generic.ICollection`1");
            var dictionaryTypeSymbol = context.SemanticModel.Compilation.GetTypeByMetadataName("System.Collections.Generic.IDictionary`2");

            var name = GetBuilderName(typeSymbol);
            var @namespace = GetBuilderNamespace(typeSymbol);

            var fullyQualifiedClassName = typeSymbol.ToString();

            // get the properties
            var properties = typeSymbol
                .GetMembers()
                .Where(m => !m.IsStatic)
                .OfType<IPropertySymbol>()
                .Select(propertySymbol => new PropertyToGenerate(propertySymbol, collectionTypeSymbol, dictionaryTypeSymbol))
                .ToImmutableArray();

            var builderFullQualitifiedName = $"{fullyQualifiedClassName}.{name}";

            return new BuilderToGenerate(name, builderFullQualitifiedName, typeSymbol.Name, fullyQualifiedClassName, @namespace, properties.WithValueSemantics());

            static string GetBuilderName(ITypeSymbol typeSymbol)
            {
                return typeSymbol.Name + "Builder";
            }

            static string GetBuilderNamespace(ITypeSymbol typeSymbol)
            {
                return typeSymbol.ContainingNamespace.IsGlobalNamespace ? string.Empty : typeSymbol.ContainingNamespace.ToString();
            }
        }

        static void Execute(
            in BuilderToGenerate builderToGenerate,
            in ImmutableArray<BuilderToGenerate> buildersToGenerate,
            Microsoft.CodeAnalysis.CSharp.LanguageVersion? languageVersion,
            SourceProductionContext context)
        {
            var generatedBuilder = InternalGenerator.GenerateNestedBuilder(builderToGenerate, buildersToGenerate);

            context.AddSource($"{builderToGenerate.ClassName}.Builder.g.cs", generatedBuilder.NormalizeWhitespace().GetText(System.Text.Encoding.UTF8));
        }
    }
}