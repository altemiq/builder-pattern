using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;


// get sample directory
var sampleDirectory = GetSampleDirectory();

// get all the syntax trees
var inputCompilation = CSharpCompilation.Create("compilation",
    Directory.EnumerateFiles(sampleDirectory, "*.cs").Select(file => CSharpSyntaxTree.ParseText(File.ReadAllText(file))),
    [
        MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
        MetadataReference.CreateFromFile(typeof(Altemiq.Patterns.Builder.GenerateBuilderAttribute).Assembly.Location)],
    new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

// Create the driver that will control the generation, passing in our generator
GeneratorDriver driver = CSharpGeneratorDriver.Create(new Altemiq.Patterns.Builder.Generator.BuilderGenerator());

// Or we can look at the results directly:
var runResult = driver
    .RunGenerators(inputCompilation)
    .GetRunResult();

if (runResult.Diagnostics is { IsEmpty: false } diagnostics)
{
    foreach (var diagnostic in diagnostics)
    {
        Console.WriteLine(diagnostic.ToString());
    }

    return;
}

foreach (var generatedTree in runResult.GeneratedTrees)
{
    Console.WriteLine(generatedTree.ToString());
}

static string GetSampleDirectory()
{
    var current = AppDomain.CurrentDomain.BaseDirectory;

    var required = Path.Combine(current, "Sample");
    while (!Directory.Exists(required))
    {
        current = Path.GetDirectoryName(current) ?? throw new InvalidOperationException();
        required = Path.Combine(current, "Sample");
    }

    return required ?? throw new InvalidOperationException();
}
