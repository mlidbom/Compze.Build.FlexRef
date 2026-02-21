using Microsoft.Build.Evaluation;

namespace Compze.Build.FlexRef.Cli;

static class ProjectFileScanner
{
    static readonly string[] DirectoriesToSkip = ["bin", "obj", "node_modules", ".git", ".vs", ".idea"];

    public static List<DiscoveredProject> ScanAllProjects(DirectoryInfo rootDirectory)
    {
        using var projectCollection = new ProjectCollection();
        return FindCsprojFilesRecursively(rootDirectory)
            .Select(csprojFile => ParseCsproj(csprojFile, projectCollection))
            .OfType<DiscoveredProject>()
            .ToList();
    }

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

    static DiscoveredProject? ParseCsproj(FileInfo csprojFile, ProjectCollection projectCollection)
    {
        try
        {
            return new DiscoveredProject(csprojFile, projectCollection);
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine($"Warning: Could not parse {csprojFile.FullName}: {exception.Message}");
            return null;
        }
    }
}
