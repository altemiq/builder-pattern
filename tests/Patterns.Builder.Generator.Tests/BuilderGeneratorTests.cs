namespace Altemiq.Patterns.Builder.Generator;

using Microsoft.CodeAnalysis;

public class BuilderGeneratorTests
{
    [Test]
    public async Task TestCaching()
    {
        var compilation = Microsoft.CodeAnalysis.CSharp.CSharpCompilation.Create("TestProject",
            [
            Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText("""
                namespace MyCode
                {
                    [Altemiq.Patterns.Builder.GenerateBuilder]
                    public partial class Test
                    {
                        public System.DateTime PrimitiveValue { get; set; }
                    }
                }
                """),
            ],
            [
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(GenerateBuilderAttribute).Assembly.Location),
            ],
            new(OutputKind.DynamicallyLinkedLibrary));

        var generator = new BuilderGenerator();
        var sourceGenerator = generator.AsSourceGenerator();

        // trackIncrementalGeneratorSteps allows to report info about each step of the generator
        GeneratorDriver driver = Microsoft.CodeAnalysis.CSharp.CSharpGeneratorDriver.Create(
            generators: [sourceGenerator],
            driverOptions: new(default, trackIncrementalGeneratorSteps: true));

        // Run the generator
        driver = driver.RunGenerators(compilation);

        // Update the compilation and rerun the generator
        compilation = compilation.AddSyntaxTrees(Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText("// dummy"));
        driver = driver.RunGenerators(compilation);

        // Assert the driver doesn't recompute the output
        var result = driver.GetRunResult().Results.Single();
        var allOutputs = result
            .TrackedOutputSteps
            .SelectMany(outputStep => outputStep.Value)
            .SelectMany(output => output.Outputs);
        await Assert.That(allOutputs).All(output => output.Reason is IncrementalStepRunReason.Cached);

        // Assert the driver use the cached result from InitialExtraction and RemovingNulls
        await Assert.That(result.TrackedSteps["InitialExtraction"].Single().Outputs).All(output => output.Reason is IncrementalStepRunReason.Unchanged);
        await Assert.That(result.TrackedSteps["RemovingNulls"].Single().Outputs).All(output => output.Reason is IncrementalStepRunReason.Unchanged or IncrementalStepRunReason.Cached);
    }

    [Test]
    public async Task GenerateExamples()
    {
        // get all the source files
        var solutionDirectory = Path.Combine(GetSolutionDirectory(), "tests", "Patterns.Builder.Examples");
        ICollection<System.Reflection.Assembly> assemblies =
        [
            typeof(GenerateBuilderAttribute).Assembly,
            typeof(InternalsVisibleTo.ClassWithInternalProperties).Assembly,
            typeof(Internal.ClassWithInternalProperties).Assembly,
        ];

        var compilation = Microsoft.CodeAnalysis.CSharp.CSharpCompilation.Create(
            "Patterns.Builder.Examples",
        Directory
                .EnumerateFiles(solutionDirectory, "*.cs", SearchOption.AllDirectories)
                .Where(file => !file.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}"))
                .Select(file => Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(File.ReadAllText(file), path: file)),
            [
                .. GetReferenceAssemblies().Assemblies.Select(file => MetadataReference.CreateFromFile(file)),
                .. assemblies.Select(assembly => MetadataReference.CreateFromFile(assembly.Location))
            ],
            new(OutputKind.DynamicallyLinkedLibrary));
        
        var generator = new BuilderGenerator();
        var sourceGenerator = generator.AsSourceGenerator();

        // trackIncrementalGeneratorSteps allows to report info about each step of the generator
        GeneratorDriver driver = Microsoft.CodeAnalysis.CSharp.CSharpGeneratorDriver.Create(
            generators: [sourceGenerator],
            driverOptions: new(default, trackIncrementalGeneratorSteps: true));

        // Run the generator
        driver = driver.RunGenerators(compilation);

        var runResult = driver.GetRunResult();
        
        // ensure we do not have any errors here
        await Assert.That(runResult.Diagnostics).IsEmpty();
    
        static string GetSolutionDirectory()
        {
            var current = Environment.CurrentDirectory;
            while (current is not null)
            {
                if (Directory.EnumerateFiles(current, "*.slnx").Any())
                {
                    return current;
                }
                
                current = Path.GetDirectoryName(current);
            }
            
            throw new FileNotFoundException();
        }
    }

    [Test]
    public async Task TestPrimitive()
    {
        var context = new CSharpSourceGeneratorTest<BuilderGenerator, DefaultVerifier>
        {
            ReferenceAssemblies = GetReferenceAssemblies(),
            TestCode =
                """
                namespace MyCode
                {
                    [Altemiq.Patterns.Builder.GenerateBuilder]
                    public partial class Test
                    {
                        [System.ComponentModel.DefaultValue(5)]
                        public int PrimitiveValue { get; set; }
                    }
                }
                """,
            TestState =
            {
                AdditionalReferences =
                {
                    // Add the attribute assembly.
                    typeof(GenerateBuilderAttribute).Assembly
                },
                GeneratedSources =
                {
                    (
                        typeof(BuilderGenerator),
                        "Test.Builder.g.cs",
                        NormalizeLineEndings(
                        """
                        // <autogenerated />
                        namespace MyCode
                        {
                            ///<content>The <see cref = "MyCode.Test"/> builder methods.</content>
                            partial class Test
                            {
                                ///<summary>Creates a new builder for a <see cref = "MyCode.Test"/> instance.</summary>
                                ///<returns>The builder for a <see cref = "MyCode.Test"/> instance.</returns>
                                public static TestBuilder CreateBuilder()
                                {
                                    return new TestBuilder();
                                }

                                ///<summary>The <see cref = "MyCode.Test"/> builder.</summary>
                                public sealed partial class TestBuilder
                                {
                                    private int primitiveValue = 5;
                                    ///<summary>Sets the <see cref = "MyCode.Test.PrimitiveValue"/> value.</summary>
                                    ///<param name = "primitiveValue">The primitive value value.</param>
                                    ///<returns>The builder for chaining.</returns>
                                    public TestBuilder WithPrimitiveValue(int primitiveValue)
                                    {
                                        this.primitiveValue = primitiveValue;
                                        return this;
                                    }
                    
                                    ///<summary>Builds an instance of <see cref = "MyCode.Test"/>.</summary>
                                    ///<returns>The instance of <see cref = "MyCode.Test"/>.</returns>
                                    public MyCode.Test Build()
                                    {
                                        var value = new MyCode.Test()
                                        {
                                            PrimitiveValue = this.primitiveValue
                                        };
                                        return value;
                                    }
                                }
                            }
                        }
                        """)),
                },
            },
        };

        await context.RunAsync(TestContext.Current?.Execution.CancellationToken ?? CancellationToken.None);
    }

    [Test]
    public async Task TestLazy()
    {
        var context = new CSharpSourceGeneratorTest<BuilderGenerator, DefaultVerifier>
        {
            ReferenceAssemblies = GetReferenceAssemblies(),
            TestCode =
                """
                namespace MyCode
                {
                    [Altemiq.Patterns.Builder.GenerateBuilder]
                    public partial class Test
                    {
                        public System.DateTime DateTimeValue { get; set; }
                    }
                }
                """,
            TestState =
            {
                AdditionalReferences =
                {
                    // Add the attribute assembly.
                    typeof(GenerateBuilderAttribute).Assembly
                },
                GeneratedSources =
                {
                    (
                        typeof(BuilderGenerator),
                        "Test.Builder.g.cs",
                        NormalizeLineEndings(
                        """
                        // <autogenerated />
                        namespace MyCode
                        {
                            ///<content>The <see cref = "MyCode.Test"/> builder methods.</content>
                            partial class Test
                            {
                                ///<summary>Creates a new builder for a <see cref = "MyCode.Test"/> instance.</summary>
                                ///<returns>The builder for a <see cref = "MyCode.Test"/> instance.</returns>
                                public static TestBuilder CreateBuilder()
                                {
                                    return new TestBuilder();
                                }

                                ///<summary>The <see cref = "MyCode.Test"/> builder.</summary>
                                public sealed partial class TestBuilder
                                {
                                    private System.Lazy<System.DateTime> dateTimeValue = new System.Lazy<System.DateTime>(() => default);
                                    ///<summary>Sets the <see cref = "MyCode.Test.DateTimeValue"/> value.</summary>
                                    ///<param name = "dateTimeValue">The date time value value.</param>
                                    ///<returns>The builder for chaining.</returns>
                                    public TestBuilder WithDateTimeValue(System.DateTime dateTimeValue)
                                    {
                                        this.dateTimeValue = new System.Lazy<System.DateTime>(() => dateTimeValue);
                                        return this;
                                    }
    
                                    ///<summary>Sets the <see cref = "MyCode.Test.DateTimeValue"/> value via a factory.</summary>
                                    ///<param name = "dateTimeValue">The date time value factory.</param>
                                    ///<returns>The builder for chaining.</returns>
                                    public TestBuilder WithDateTimeValue(System.Func<System.DateTime> dateTimeValue)
                                    {
                                        this.dateTimeValue = new System.Lazy<System.DateTime>(dateTimeValue);
                                        return this;
                                    }
                        
                                    ///<summary>Sets the <see cref = "MyCode.Test.DateTimeValue"/> value via a constructor.</summary>
                                    ///<param name = "date">The date part.</param>
                                    ///<param name = "time">The time part.</param>
                                    ///<returns>The builder for chaining.</returns>
                                    public TestBuilder WithDateTimeValue(System.DateOnly date, System.TimeOnly time)
                                    {
                                        this.dateTimeValue = new System.Lazy<System.DateTime>(() => new System.DateTime(date, time));
                                        return this;
                                    }
                        
                                    ///<summary>Sets the <see cref = "MyCode.Test.DateTimeValue"/> value via a constructor.</summary>
                                    ///<param name = "date">The date part.</param>
                                    ///<param name = "time">The time part.</param>
                                    ///<param name = "kind">One of the enumeration values that indicates whether <paramref name = "date"/> and <paramref name = "time"/> specify a local time, Coordinated Universal Time (UTC), or neither.</param>
                                    ///<returns>The builder for chaining.</returns>
                                    public TestBuilder WithDateTimeValue(System.DateOnly date, System.TimeOnly time, System.DateTimeKind kind)
                                    {
                                        this.dateTimeValue = new System.Lazy<System.DateTime>(() => new System.DateTime(date, time, kind));
                                        return this;
                                    }
 
                                    ///<summary>Sets the <see cref = "MyCode.Test.DateTimeValue"/> value via a constructor.</summary>
                                    ///<param name = "year">The year (1 through 9999).</param>
                                    ///<param name = "month">The month (1 through 12).</param>
                                    ///<param name = "day">The day (1 through the number of days in <paramref name = "month"/>).</param>
                                    ///<returns>The builder for chaining.</returns>
                                    public TestBuilder WithDateTimeValue(int year, int month, int day)
                                    {
                                        this.dateTimeValue = new System.Lazy<System.DateTime>(() => new System.DateTime(year, month, day));
                                        return this;
                                    }
                        
                                    ///<summary>Sets the <see cref = "MyCode.Test.DateTimeValue"/> value via a constructor.</summary>
                                    ///<param name = "year">The year (1 through the number of years in <paramref name = "calendar"/>).</param>
                                    ///<param name = "month">The month (1 through the number of months in <paramref name = "calendar"/>).</param>
                                    ///<param name = "day">The day (1 through the number of days in <paramref name = "month"/>).</param>
                                    ///<param name = "calendar">The calendar that is used to interpret <paramref name = "year"/>, <paramref name = "month"/>, and <paramref name = "day"/>.</param>
                                    ///<returns>The builder for chaining.</returns>
                                    public TestBuilder WithDateTimeValue(int year, int month, int day, System.Globalization.Calendar calendar)
                                    {
                                        this.dateTimeValue = new System.Lazy<System.DateTime>(() => new System.DateTime(year, month, day, calendar));
                                        return this;
                                    }
                        
                                    ///<summary>Sets the <see cref = "MyCode.Test.DateTimeValue"/> value via a constructor.</summary>
                                    ///<param name = "year">The year (1 through 9999).</param>
                                    ///<param name = "month">The month (1 through 12).</param>
                                    ///<param name = "day">The day (1 through the number of days in <paramref name = "month"/>).</param>
                                    ///<param name = "hour">The hours (0 through 23).</param>
                                    ///<param name = "minute">The minutes (0 through 59).</param>
                                    ///<param name = "second">The seconds (0 through 59).</param>
                                    ///<returns>The builder for chaining.</returns>
                                    public TestBuilder WithDateTimeValue(int year, int month, int day, int hour, int minute, int second)
                                    {
                                        this.dateTimeValue = new System.Lazy<System.DateTime>(() => new System.DateTime(year, month, day, hour, minute, second));
                                        return this;
                                    }
                        
                                    ///<summary>Sets the <see cref = "MyCode.Test.DateTimeValue"/> value via a constructor.</summary>
                                    ///<param name = "year">The year (1 through 9999).</param>
                                    ///<param name = "month">The month (1 through 12).</param>
                                    ///<param name = "day">The day (1 through the number of days in <paramref name = "month"/>).</param>
                                    ///<param name = "hour">The hours (0 through 23).</param>
                                    ///<param name = "minute">The minutes (0 through 59).</param>
                                    ///<param name = "second">The seconds (0 through 59).</param>
                                    ///<param name = "kind">One of the enumeration values that indicates whether <paramref name = "year"/>, <paramref name = "month"/>, <paramref name = "day"/>, <paramref name = "hour"/>, <paramref name = "minute"/> and <paramref name = "second"/> specify a local time, Coordinated Universal Time (UTC), or neither.</param>
                                    ///<returns>The builder for chaining.</returns>
                                    public TestBuilder WithDateTimeValue(int year, int month, int day, int hour, int minute, int second, System.DateTimeKind kind)
                                    {
                                        this.dateTimeValue = new System.Lazy<System.DateTime>(() => new System.DateTime(year, month, day, hour, minute, second, kind));
                                        return this;
                                    }
                        
                                    ///<summary>Sets the <see cref = "MyCode.Test.DateTimeValue"/> value via a constructor.</summary>
                                    ///<param name = "year">The year (1 through the number of years in <paramref name = "calendar"/>).</param>
                                    ///<param name = "month">The month (1 through the number of months in <paramref name = "calendar"/>).</param>
                                    ///<param name = "day">The day (1 through the number of days in <paramref name = "month"/>).</param>
                                    ///<param name = "hour">The hours (0 through 23).</param>
                                    ///<param name = "minute">The minutes (0 through 59).</param>
                                    ///<param name = "second">The seconds (0 through 59).</param>
                                    ///<param name = "calendar">The calendar that is used to interpret <paramref name = "year"/>, <paramref name = "month"/>, and <paramref name = "day"/>.</param>
                                    ///<returns>The builder for chaining.</returns>
                                    public TestBuilder WithDateTimeValue(int year, int month, int day, int hour, int minute, int second, System.Globalization.Calendar calendar)
                                    {
                                        this.dateTimeValue = new System.Lazy<System.DateTime>(() => new System.DateTime(year, month, day, hour, minute, second, calendar));
                                        return this;
                                    }
                        
                                    ///<summary>Sets the <see cref = "MyCode.Test.DateTimeValue"/> value via a constructor.</summary>
                                    ///<param name = "year">The year (1 through 9999).</param>
                                    ///<param name = "month">The month (1 through 12).</param>
                                    ///<param name = "day">The day (1 through the number of days in <paramref name = "month"/>).</param>
                                    ///<param name = "hour">The hours (0 through 23).</param>
                                    ///<param name = "minute">The minutes (0 through 59).</param>
                                    ///<param name = "second">The seconds (0 through 59).</param>
                                    ///<param name = "millisecond">The milliseconds (0 through 999).</param>
                                    ///<returns>The builder for chaining.</returns>
                                    public TestBuilder WithDateTimeValue(int year, int month, int day, int hour, int minute, int second, int millisecond)
                                    {
                                        this.dateTimeValue = new System.Lazy<System.DateTime>(() => new System.DateTime(year, month, day, hour, minute, second, millisecond));
                                        return this;
                                    }
                        
                                    ///<summary>Sets the <see cref = "MyCode.Test.DateTimeValue"/> value via a constructor.</summary>
                                    ///<param name = "year">The year (1 through 9999).</param>
                                    ///<param name = "month">The month (1 through 12).</param>
                                    ///<param name = "day">The day (1 through the number of days in <paramref name = "month"/>).</param>
                                    ///<param name = "hour">The hours (0 through 23).</param>
                                    ///<param name = "minute">The minutes (0 through 59).</param>
                                    ///<param name = "second">The seconds (0 through 59).</param>
                                    ///<param name = "millisecond">The milliseconds (0 through 999).</param>
                                    ///<param name = "kind">One of the enumeration values that indicates whether <paramref name = "year"/>, <paramref name = "month"/>, <paramref name = "day"/>, <paramref name = "hour"/>, <paramref name = "minute"/>, <paramref name = "second"/>, and <paramref name = "millisecond"/> specify a local time, Coordinated Universal Time (UTC), or neither.</param>
                                    ///<returns>The builder for chaining.</returns>
                                    public TestBuilder WithDateTimeValue(int year, int month, int day, int hour, int minute, int second, int millisecond, System.DateTimeKind kind)
                                    {
                                        this.dateTimeValue = new System.Lazy<System.DateTime>(() => new System.DateTime(year, month, day, hour, minute, second, millisecond, kind));
                                        return this;
                                    }
                        
                                    ///<summary>Sets the <see cref = "MyCode.Test.DateTimeValue"/> value via a constructor.</summary>
                                    ///<param name = "year">The year (1 through the number of years in <paramref name = "calendar"/>).</param>
                                    ///<param name = "month">The month (1 through the number of months in <paramref name = "calendar"/>).</param>
                                    ///<param name = "day">The day (1 through the number of days in <paramref name = "month"/>).</param>
                                    ///<param name = "hour">The hours (0 through 23).</param>
                                    ///<param name = "minute">The minutes (0 through 59).</param>
                                    ///<param name = "second">The seconds (0 through 59).</param>
                                    ///<param name = "millisecond">The milliseconds (0 through 999).</param>
                                    ///<param name = "calendar">The calendar that is used to interpret <paramref name = "year"/>, <paramref name = "month"/>, and <paramref name = "day"/>.</param>
                                    ///<returns>The builder for chaining.</returns>
                                    public TestBuilder WithDateTimeValue(int year, int month, int day, int hour, int minute, int second, int millisecond, System.Globalization.Calendar calendar)
                                    {
                                        this.dateTimeValue = new System.Lazy<System.DateTime>(() => new System.DateTime(year, month, day, hour, minute, second, millisecond, calendar));
                                        return this;
                                    }
                        
                                    ///<summary>Sets the <see cref = "MyCode.Test.DateTimeValue"/> value via a constructor.</summary>
                                    ///<param name = "year">The year (1 through the number of years in <paramref name = "calendar"/>).</param>
                                    ///<param name = "month">The month (1 through the number of months in <paramref name = "calendar"/>).</param>
                                    ///<param name = "day">The day (1 through the number of days in <paramref name = "month"/>).</param>
                                    ///<param name = "hour">The hours (0 through 23).</param>
                                    ///<param name = "minute">The minutes (0 through 59).</param>
                                    ///<param name = "second">The seconds (0 through 59).</param>
                                    ///<param name = "millisecond">The milliseconds (0 through 999).</param>
                                    ///<param name = "calendar">The calendar that is used to interpret <paramref name = "year"/>, <paramref name = "month"/>, and <paramref name = "day"/>.</param>
                                    ///<param name = "kind">One of the enumeration values that indicates whether <paramref name = "year"/>, <paramref name = "month"/>, <paramref name = "day"/>, <paramref name = "hour"/>, <paramref name = "minute"/>, <paramref name = "second"/>, and <paramref name = "millisecond"/> specify a local time, Coordinated Universal Time (UTC), or neither.</param>
                                    ///<returns>The builder for chaining.</returns>
                                    public TestBuilder WithDateTimeValue(int year, int month, int day, int hour, int minute, int second, int millisecond, System.Globalization.Calendar calendar, System.DateTimeKind kind)
                                    {
                                        this.dateTimeValue = new System.Lazy<System.DateTime>(() => new System.DateTime(year, month, day, hour, minute, second, millisecond, calendar, kind));
                                        return this;
                                    }
                        
                                    ///<summary>Sets the <see cref = "MyCode.Test.DateTimeValue"/> value via a constructor.</summary>
                                    ///<param name = "year">The year (1 through 9999).</param>
                                    ///<param name = "month">The month (1 through 12).</param>
                                    ///<param name = "day">The day (1 through the number of days in <paramref name = "month"/>).</param>
                                    ///<param name = "hour">The hours (0 through 23).</param>
                                    ///<param name = "minute">The minutes (0 through 59).</param>
                                    ///<param name = "second">The seconds (0 through 59).</param>
                                    ///<param name = "millisecond">The milliseconds (0 through 999).</param>
                                    ///<param name = "microsecond">The microseconds (0 through 999).</param>
                                    ///<returns>The builder for chaining.</returns>
                                    public TestBuilder WithDateTimeValue(int year, int month, int day, int hour, int minute, int second, int millisecond, int microsecond)
                                    {
                                        this.dateTimeValue = new System.Lazy<System.DateTime>(() => new System.DateTime(year, month, day, hour, minute, second, millisecond, microsecond));
                                        return this;
                                    }

                                    ///<summary>Sets the <see cref = "MyCode.Test.DateTimeValue"/> value via a constructor.</summary>
                                    ///<param name = "year">The year (1 through 9999).</param>
                                    ///<param name = "month">The month (1 through 12).</param>
                                    ///<param name = "day">The day (1 through the number of days in <paramref name = "month"/>).</param>
                                    ///<param name = "hour">The hours (0 through 23).</param>
                                    ///<param name = "minute">The minutes (0 through 59).</param>
                                    ///<param name = "second">The seconds (0 through 59).</param>
                                    ///<param name = "millisecond">The milliseconds (0 through 999).</param>
                                    ///<param name = "microsecond">The microseconds (0 through 999).</param>
                                    ///<param name = "kind">One of the enumeration values that indicates whether <paramref name = "year"/>, <paramref name = "month"/>, <paramref name = "day"/>, <paramref name = "hour"/>, <paramref name = "minute"/>, <paramref name = "second"/>, and <paramref name = "millisecond"/> specify a local time, Coordinated Universal Time (UTC), or neither.</param>
                                    ///<returns>The builder for chaining.</returns>
                                    public TestBuilder WithDateTimeValue(int year, int month, int day, int hour, int minute, int second, int millisecond, int microsecond, System.DateTimeKind kind)
                                    {
                                        this.dateTimeValue = new System.Lazy<System.DateTime>(() => new System.DateTime(year, month, day, hour, minute, second, millisecond, microsecond, kind));
                                        return this;
                                    }

                                    ///<summary>Sets the <see cref = "MyCode.Test.DateTimeValue"/> value via a constructor.</summary>
                                    ///<param name = "year">The year (1 through the number of years in <paramref name = "calendar"/>).</param>
                                    ///<param name = "month">The month (1 through the number of months in <paramref name = "calendar"/>).</param>
                                    ///<param name = "day">The day (1 through the number of days in <paramref name = "month"/>).</param>
                                    ///<param name = "hour">The hours (0 through 23).</param>
                                    ///<param name = "minute">The minutes (0 through 59).</param>
                                    ///<param name = "second">The seconds (0 through 59).</param>
                                    ///<param name = "millisecond">The milliseconds (0 through 999).</param>
                                    ///<param name = "microsecond">The microseconds (0 through 999).</param>
                                    ///<param name = "calendar">The calendar that is used to interpret <paramref name = "year"/>, <paramref name = "month"/>, and <paramref name = "day"/>.</param>
                                    ///<returns>The builder for chaining.</returns>
                                    public TestBuilder WithDateTimeValue(int year, int month, int day, int hour, int minute, int second, int millisecond, int microsecond, System.Globalization.Calendar calendar)
                                    {
                                        this.dateTimeValue = new System.Lazy<System.DateTime>(() => new System.DateTime(year, month, day, hour, minute, second, millisecond, microsecond, calendar));
                                        return this;
                                    }
                        
                                    ///<summary>Sets the <see cref = "MyCode.Test.DateTimeValue"/> value via a constructor.</summary>
                                    ///<param name = "year">The year (1 through the number of years in <paramref name = "calendar"/>).</param>
                                    ///<param name = "month">The month (1 through the number of months in <paramref name = "calendar"/>).</param>
                                    ///<param name = "day">The day (1 through the number of days in <paramref name = "month"/>).</param>
                                    ///<param name = "hour">The hours (0 through 23).</param>
                                    ///<param name = "minute">The minutes (0 through 59).</param>
                                    ///<param name = "second">The seconds (0 through 59).</param>
                                    ///<param name = "millisecond">The milliseconds (0 through 999).</param>
                                    ///<param name = "microsecond">The microseconds (0 through 999).</param>
                                    ///<param name = "calendar">The calendar that is used to interpret <paramref name = "year"/>, <paramref name = "month"/>, and <paramref name = "day"/>.</param>
                                    ///<param name = "kind">One of the enumeration values that indicates whether <paramref name = "year"/>, <paramref name = "month"/>, <paramref name = "day"/>, <paramref name = "hour"/>, <paramref name = "minute"/>, <paramref name = "second"/>, and <paramref name = "millisecond"/> specify a local time, Coordinated Universal Time (UTC), or neither.</param>
                                    ///<returns>The builder for chaining.</returns>
                                    public TestBuilder WithDateTimeValue(int year, int month, int day, int hour, int minute, int second, int millisecond, int microsecond, System.Globalization.Calendar calendar, System.DateTimeKind kind)
                                    {
                                        this.dateTimeValue = new System.Lazy<System.DateTime>(() => new System.DateTime(year, month, day, hour, minute, second, millisecond, microsecond, calendar, kind));
                                        return this;
                                    }
                        
                                    ///<summary>Sets the <see cref = "MyCode.Test.DateTimeValue"/> value via a constructor.</summary>
                                    ///<param name = "ticks">A date and time expressed in the number of 100-nanosecond intervals that have elapsed since January 1, 0001 at 00:00:00.000 in the Gregorian calendar.</param>
                                    ///<returns>The builder for chaining.</returns>
                                    public TestBuilder WithDateTimeValue(long ticks)
                                    {
                                        this.dateTimeValue = new System.Lazy<System.DateTime>(() => new System.DateTime(ticks));
                                        return this;
                                    }
                        
                                    ///<summary>Sets the <see cref = "MyCode.Test.DateTimeValue"/> value via a constructor.</summary>
                                    ///<param name = "ticks">A date and time expressed in the number of 100-nanosecond intervals that have elapsed since January 1, 0001 at 00:00:00.000 in the Gregorian calendar.</param>
                                    ///<param name = "kind">One of the enumeration values that indicates whether <paramref name = "ticks"/> specifies a local time, Coordinated Universal Time (UTC), or neither.</param>
                                    ///<returns>The builder for chaining.</returns>
                                    public TestBuilder WithDateTimeValue(long ticks, System.DateTimeKind kind)
                                    {
                                        this.dateTimeValue = new System.Lazy<System.DateTime>(() => new System.DateTime(ticks, kind));
                                        return this;
                                    }
                    
                                    ///<summary>Builds an instance of <see cref = "MyCode.Test"/>.</summary>
                                    ///<returns>The instance of <see cref = "MyCode.Test"/>.</returns>
                                    public MyCode.Test Build()
                                    {
                                        var value = new MyCode.Test()
                                        {
                                            DateTimeValue = this.dateTimeValue.Value
                                        };
                                        return value;
                                    }
                                }
                            }
                        }
                        """)),
                },
            },
        };

        await context.RunAsync(TestContext.Current?.Execution.CancellationToken ?? CancellationToken.None);
    }

    private static string NormalizeLineEndings(string input) => input
        .Replace("\r\n", "\n")  // collapse existing CRLF → LF
        .Replace("\r", "\n")    // fix any stray CR
        .Replace("\n", Constants.NewLine); // convert to generator new line

    private static ReferenceAssemblies GetReferenceAssemblies()
    {
#if NET10_0_OR_GREATER
        return ReferenceAssemblies.Net.Net100;
#elif NET9_0_OR_GREATER
        return ReferenceAssemblies.Net.Net90;
#elif NET8_0_OR_GREATER
        return ReferenceAssemblies.Net.Net80;
#elif NET7_0_OR_GREATER
        return ReferenceAssemblies.Net.Net70;
#elif NET6_0_OR_GREATER
        return ReferenceAssemblies.Net.Net60;
#elif NET5_0_OR_GREATER
        return ReferenceAssemblies.Net.Net50;
#elif NETCOREAPP3_1_OR_GREATER
        return ReferenceAssemblies.NetCore.NetCoreApp31;
#elif NETCOREAPP3_0_OR_GREATER
        return ReferenceAssemblies.NetCore.NetCoreApp30;
#elif NETCOREAPP2_1_OR_GREATER
        return ReferenceAssemblies.NetCore.NetCoreApp21;
#elif NETCOREAPP2_0_OR_GREATER
        return ReferenceAssemblies.NetCore.NetCoreApp20;
#elif NETCOREAPP1_1_OR_GREATER
        return ReferenceAssemblies.NetCore.NetCoreApp11;
#elif NETCOREAPP1_0_OR_GREATER
        return ReferenceAssemblies.NetCore.NetCoreApp10;
#endif
    }
}