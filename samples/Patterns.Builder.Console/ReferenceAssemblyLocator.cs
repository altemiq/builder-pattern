using Microsoft.CodeAnalysis;

namespace Patterns.Builder.Console;

public class ReferenceAssemblyLocator
{
    public static IEnumerable<MetadataReference> GetNetCoreReferences(
        string targetFramework, // e.g., "net6.0", "net8.0", "netstandard2.0"
        bool includeXmlDocs = true)
    {
        // Map TFM -> pack id(s)
        if (PacksForTfm(targetFramework).ToList() is not { Count: not 0 } packs)
        {
            throw new NotSupportedException($"Unknown or unsupported TFM: {targetFramework}");
        }

        var dotnetRoot = GetDotNetRoot();

        var any = false;
        foreach (var packId in packs)
        {
            var refDir = FindPackRefDirectory(dotnetRoot, packId, targetFramework);
            if (refDir is null)
            {
                // not installed; skip (or you can throw)
                continue;
            }

            foreach (var dll in Directory.EnumerateFiles(refDir, "*.dll"))
            {
                DocumentationProvider? doc = null;
                if (includeXmlDocs)
                {
                    var xml = Path.ChangeExtension(dll, ".xml");
                    if (File.Exists(xml))
                    {
                        doc = XmlDocumentationProvider.CreateFromFile(xml);
                    }
                }

                any = true;
                yield return MetadataReference.CreateFromFile(dll, documentation: doc);
            }
        }

        if (any)
        {
            yield break;
        }

        throw new InvalidOperationException($"No reference assemblies found for {targetFramework}. Ensure the targeting pack is installed.");
    }

    private static IEnumerable<string> PacksForTfm(string tfm)
    {
        // Core framework
        if (tfm.StartsWith("net", StringComparison.OrdinalIgnoreCase) &&
            !tfm.StartsWith("netstandard", StringComparison.OrdinalIgnoreCase) &&
            !tfm.StartsWith("net4", StringComparison.OrdinalIgnoreCase))
        {
            yield return "Microsoft.NETCore.App.Ref";

            // Windows desktop optional pack for netX.Y-windows
            if (tfm.Contains("-windows", StringComparison.OrdinalIgnoreCase))
                yield return "Microsoft.WindowsDesktop.App.Ref";
        }
        else if (tfm.StartsWith("netstandard2.0", StringComparison.OrdinalIgnoreCase) ||
                 tfm.StartsWith("netstandard2.1", StringComparison.OrdinalIgnoreCase))
        {
            yield return "NETStandard.Library.Ref";
        }
    }

    private static string GetDotNetRoot()
    {
        var dotnetRoot = Environment.GetEnvironmentVariable("DOTNET_ROOT");
        if (!string.IsNullOrWhiteSpace(dotnetRoot) && Directory.Exists(dotnetRoot))
        {
            return dotnetRoot;
        }

        if (OperatingSystem.IsWindows())
        {
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "dotnet");
            if (Directory.Exists(path))
            {
                return path;
            }
        }
        else
        {
            // Typical Linux/macOS
            var path = "/usr/share/dotnet";
            if (Directory.Exists(path))
            {
                return path;
            }

            path = "/usr/local/share/dotnet";
            if (Directory.Exists(path))
            {
                return path;
            }
        }

        throw new DirectoryNotFoundException(
            "Could not locate DOTNET_ROOT. Install the .NET SDK or set DOTNET_ROOT.");
    }

    private static string? FindPackRefDirectory(string dotnetRoot, string packId, string tfm)
    {
        var packRoot = Path.Combine(dotnetRoot, "packs", packId);
        if (!Directory.Exists(packRoot))
        {
            return null;
        }

        // Pick the highest version installed
        return Directory
            .GetDirectories(packRoot)
            .OrderByDescending(Path.GetFileName)
            .Select(versionFolder => Path.Combine(versionFolder, "ref", tfm))
            .FirstOrDefault(Directory.Exists);
    }
}