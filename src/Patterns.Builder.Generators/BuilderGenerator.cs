// -----------------------------------------------------------------------
// <copyright file="BuilderGenerator.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.Patterns.Builder.Generators;

using System.Collections.Immutable;
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
        AttachToDebugger(TimeSpan.FromSeconds(300));

        var compilationDetails = context.CompilationProvider
            .Select((x, _) => x is CSharpCompilation compilation ? compilation.LanguageVersion : default);

        var nestedBuildersToGenerate =
            context
                .SyntaxProvider
                .ForAttributeWithMetadataName(
                    TypeNames.Markers.GenerateBuilderAttribute,
                    predicate: (node, _) => node is ClassDeclarationSyntax or StructDeclarationSyntax or RecordDeclarationSyntax,
                    transform: GetNestedTypeToGenerate)
                .WithTrackingName(TrackingNames.InitialExtraction)
                .Where(static b => b is not null)
                .Select(static (b, _) => b!.Value)
                .WithTrackingName(TrackingNames.RemovingNulls);

        var externalBuildersToGenerate =
            context
                .SyntaxProvider
                .ForAttributeWithMetadataName(
                    TypeNames.Markers.GenerateBuilderForAttribute,
                    predicate: (node, _) => node is ClassDeclarationSyntax,
                    transform: GetExternalTypeToGenerate)
                .WithTrackingName(TrackingNames.InitialExternalExtraction)
                .Where(static b => b is not null)
                .Select(static (b, _) => b!.Value)
                .WithTrackingName(TrackingNames.RemovingNulls);

        var allBuilders = nestedBuildersToGenerate
            .Collect()
            .Combine(externalBuildersToGenerate.Collect())
            .Select((value, _) => value.Left.AddRange(value.Right));

        context.RegisterSourceOutput(
            nestedBuildersToGenerate.Combine(allBuilders).Combine(compilationDetails),
            (spc, source) => CreateNestedBuilder(source.Left.Left, source.Left.Right, source.Right, spc));

        context.RegisterSourceOutput(
            externalBuildersToGenerate.Combine(allBuilders).Combine(compilationDetails),
            (spc, source) => CreateExternalBuilder(source.Left.Left, source.Left.Right, source.Right, spc));

        static BuilderToGenerate? GetNestedTypeToGenerate(GeneratorAttributeSyntaxContext context, CancellationToken cancellationToken)
        {
            if (context.TargetSymbol is not ITypeSymbol typeSymbol)
            {
                return null;
            }

            cancellationToken.ThrowIfCancellationRequested();

            var collectionTypeSymbol = context.SemanticModel.Compilation.GetTypeByMetadataName("System.Collections.Generic.ICollection`1");
            var dictionaryTypeSymbol = context.SemanticModel.Compilation.GetTypeByMetadataName("System.Collections.Generic.IDictionary`2");

            var name = typeSymbol.Name + "Builder";
            var fullyQualifiedClassName = typeSymbol.ToString();

            // get the properties
            var properties = typeSymbol
                .GetMembers()
                .Where(m => !m.IsStatic)
                .OfType<IPropertySymbol>()
                .Select(propertySymbol => PropertyToGenerate.Create(propertySymbol, collectionTypeSymbol, dictionaryTypeSymbol))
                .ToImmutableArray();

            return new BuilderToGenerate(
                name,
                $"{fullyQualifiedClassName}.{name}",
                typeSymbol.Name,
                fullyQualifiedClassName,
                GetClassSyntaxKind(context.TargetNode),
                GetBuilderNamespace(typeSymbol),
                properties.WithValueSemantics());

            static string GetBuilderNamespace(ITypeSymbol typeSymbol)
            {
                return typeSymbol.ContainingNamespace.IsGlobalNamespace ? string.Empty : typeSymbol.ContainingNamespace.ToString();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Minor Code Smell", "S3267:Loops should be simplified with \"LINQ\" expressions", Justification = "This has a pattern match in it.")]
        static BuilderToGenerate? GetExternalTypeToGenerate(GeneratorAttributeSyntaxContext context, CancellationToken cancellationToken)
        {
            if (context.TargetSymbol is not ITypeSymbol builderTypeSymbol)
            {
                return default;
            }

            foreach (var attribute in context.Attributes)
            {
                if (attribute.AttributeClass is not { Name: var name, IsGenericType: true, TypeArguments: [var classTypeSymbol] }
                    || (!StringComparer.Ordinal.Equals(name, TypeNames.GenerateBuilderForAttributeShortName) && !StringComparer.Ordinal.Equals(name, TypeNames.GenerateBuilderForAttributeLongName)))
                {
                    continue;
                }

                cancellationToken.ThrowIfCancellationRequested();

                var collectionTypeSymbol = context.SemanticModel.Compilation.GetTypeByMetadataName("System.Collections.Generic.ICollection`1");
                var dictionaryTypeSymbol = context.SemanticModel.Compilation.GetTypeByMetadataName("System.Collections.Generic.IDictionary`2");

                var internalsVisiblyTo = classTypeSymbol.ContainingAssembly.GivesAccessTo(builderTypeSymbol.ContainingAssembly);

                // get the properties
                var properties = classTypeSymbol
                    .GetMembers()
                    .Where(m => !m.IsStatic)
                    .OfType<IPropertySymbol>()
                    .Select(propertySymbol => PropertyToGenerate.Create(propertySymbol, collectionTypeSymbol, dictionaryTypeSymbol))
                    .Where(property => (internalsVisiblyTo && property.Accessibility is SyntaxKind.InternalKeyword) || property.Accessibility is SyntaxKind.PublicKeyword)
                    .ToImmutableArray();

                return new BuilderToGenerate(
                    builderTypeSymbol.Name,
                    builderTypeSymbol.ToString(),
                    classTypeSymbol.Name,
                    classTypeSymbol.ToString(),
                    GetClassSyntaxKind(context.TargetNode),
                    GetBuilderNamespace(builderTypeSymbol),
                    properties.WithValueSemantics());

                static string GetBuilderNamespace(ITypeSymbol typeSymbol)
                {
                    return typeSymbol.ContainingNamespace.IsGlobalNamespace ? string.Empty : typeSymbol.ContainingNamespace.ToString();
                }
            }

            return default;
        }

        static void CreateNestedBuilder(
            in BuilderToGenerate builderToGenerate,
            in ImmutableArray<BuilderToGenerate> buildersToGenerate,
            LanguageVersion? languageVersion,
            SourceProductionContext context)
        {
            var useCollectionExpressions = languageVersion is not LanguageVersion.Preview and >= LanguageVersion.CSharp12; // C#12

            var generatedBuilder = InternalGenerator.GenerateNestedBuilder(builderToGenerate, buildersToGenerate, useCollectionExpressions);

            context.AddSource($"{builderToGenerate.ClassName}.Builder.g.cs", generatedBuilder.NormalizeWhitespace(eol: Constants.NewLine).GetText(System.Text.Encoding.UTF8));
        }

        static void CreateExternalBuilder(
            in BuilderToGenerate builderToGenerate,
            in ImmutableArray<BuilderToGenerate> buildersToGenerate,
            LanguageVersion? languageVersion,
            SourceProductionContext context)
        {
            var useCollectionExpressions = languageVersion is not LanguageVersion.Preview and >= LanguageVersion.CSharp12; // C#12

            var generatedBuilder = InternalGenerator.GenerateExternalBuilder(builderToGenerate, buildersToGenerate, useCollectionExpressions);

            context.AddSource($"{builderToGenerate.BuilderName}.g.cs", generatedBuilder.NormalizeWhitespace(eol: Constants.NewLine).GetText(System.Text.Encoding.UTF8));
        }

        static SyntaxKind GetClassSyntaxKind(SyntaxNode targetNode)
        {
            return targetNode switch
            {
                ClassDeclarationSyntax => SyntaxKind.ClassDeclaration,
                StructDeclarationSyntax => SyntaxKind.StructDeclaration,
                RecordDeclarationSyntax recordDeclaration when recordDeclaration.ClassOrStructKeyword.Kind() is SyntaxKind.StructKeyword => SyntaxKind.RecordStructDeclaration,
                RecordDeclarationSyntax => SyntaxKind.RecordDeclaration,
                _ => throw new NotSupportedException(),
            };
        }
    }

    [System.Diagnostics.Conditional("DEBUG_SOURCE_GENERATORS")]
    private static void AttachToDebugger(TimeSpan? limit = null)
    {
        if (System.Diagnostics.Debugger.IsAttached)
        {
            System.Diagnostics.Debugger.Break();
            return;
        }

        if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows)
            && System.Diagnostics.Debugger.Launch())
        {
            return;
        }

        limit ??= TimeSpan.FromSeconds(30);
        var source = new CancellationTokenSource(limit.Value);

        try
        {
            while (!System.Diagnostics.Debugger.IsAttached && !source.IsCancellationRequested)
            {
                Thread.Sleep(100);
            }
        }
        finally
        {
            source.Dispose();
        }
    }
}