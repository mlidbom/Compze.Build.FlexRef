using System.Xml.Linq;

namespace Compze.Build.FlexRef.Cli;

static class ProjectFileScanner
{
    static readonly string[] DirectoriesToSkip = ["bin", "obj", "node_modules", ".git", ".vs", ".idea"];

    public static List<DiscoveredProject> ScanAllProjects(DirectoryInfo rootDirectory) =>
        FindCsprojFilesRecursively(rootDirectory)
           .Select(ParseSingleCsproj)
           .OfType<DiscoveredProject>()
           .ToList();

    static IEnumerable<FileInfo> FindCsprojFilesRecursively(DirectoryInfo directory)
    {
        foreach (var file in directory.GetFiles("*.csproj"))
            yield return file;

        foreach (var subdirectory in directory.GetDirectories())
        {
            if (DirectoriesToSkip.Contains(subdirectory.Name, StringComparer.OrdinalIgnoreCase))
                continue;

            foreach (var file in FindCsprojFilesRecursively(subdirectory))
                yield return file;
        }
    }

    static DiscoveredProject? ParseSingleCsproj(FileInfo csprojFile)
    {
        try
        {
            var document = XDocument.Load(csprojFile.FullName);
            var rootElement = document.Root;
            if (rootElement == null) return null;

            var explicitPackageId = rootElement.Descendants("PackageId").FirstOrDefault()?.Value;
            var isPackableValue = rootElement.Descendants("IsPackable").FirstOrDefault()?.Value;
            var isExplicitlyNotPackable = string.Equals(isPackableValue, "false", StringComparison.OrdinalIgnoreCase);
            var isExplicitlyPackable = string.Equals(isPackableValue, "true", StringComparison.OrdinalIgnoreCase);
            var isPackable = !isExplicitlyNotPackable && (explicitPackageId != null || isExplicitlyPackable);

            var effectivePackageId = explicitPackageId
                ?? (isPackable ? Path.GetFileNameWithoutExtension(csprojFile.Name) : null);

            var projectReferences = rootElement.Descendants("ProjectReference")
                .Select(element => element.Attribute("Include")?.Value)
                .Where(includePath => includePath != null)
                .Select(includePath => new DiscoveredProjectReference(includePath!, Path.GetFileName(includePath!)))
                .ToList();

            var packageReferences = rootElement.Descendants("PackageReference")
                .Select(element => (
                    Name: element.Attribute("Include")?.Value,
                    Version: element.Attribute("Version")?.Value ?? ""))
                .Where(pair => pair.Name != null)
                .Select(pair => new DiscoveredPackageReference(pair.Name!, pair.Version))
                .ToList();

            return new DiscoveredProject(
                CsprojFullPath: csprojFile.FullName,
                CsprojFileName: csprojFile.Name,
                PackageId: effectivePackageId,
                IsPackable: isPackable,
                ProjectReferences: projectReferences,
                PackageReferences: packageReferences);
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine($"Warning: Could not parse {csprojFile.FullName}: {exception.Message}");
            return null;
        }
    }
}
