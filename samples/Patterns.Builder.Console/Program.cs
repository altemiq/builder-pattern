using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;


// get sample directory
var sampleDirectory = GetSampleDirectory();

// get all the syntax trees
IList<SyntaxTree> syntaxTrees = [];
foreach (var file in Directory.EnumerateFiles(sampleDirectory, "*.cs"))
{
    syntaxTrees.Add(CSharpSyntaxTree.ParseText(File.ReadAllText( file)));
}

var inputCompilation = CSharpCompilation.Create("compilation",
    syntaxTrees,
    [
        MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
        MetadataReference.CreateFromFile(typeof(Altemiq.Patterns.Builder.GenerateBuilderAttribute).Assembly.Location)],
    new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

var generator = new Altemiq.Patterns.Builder.Generator.BuilderGenerator();

// Create the driver that will control the generation, passing in our generator
GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

driver = driver
    .RunGeneratorsAndUpdateCompilation(
        inputCompilation,
        out var outputCompilation,
        out var diagnostics);

// Or we can look at the results directly:
var runResult = driver.GetRunResult();

var output = runResult.GeneratedTrees.First().ToString();

Console.WriteLine(output);

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
