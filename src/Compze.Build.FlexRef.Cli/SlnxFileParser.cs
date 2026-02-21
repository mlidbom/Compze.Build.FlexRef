using System.Xml.Linq;

namespace Compze.Build.FlexRef.Cli;

record SlnxSolutionInfo(
    string SlnxFullPath,
    List<string> ProjectFileNames);

static class SlnxFileParser
{
    static readonly string[] DirectoriesToSkip = ["bin", "obj", "node_modules", ".git", ".vs", ".idea"];

    public static List<SlnxSolutionInfo> FindAndParseAllSolutions(DirectoryInfo rootDirectory)
    {
        var solutions = new List<SlnxSolutionInfo>();
        foreach (var slnxFile in FindSlnxFilesRecursively(rootDirectory))
        {
            var solution = ParseSingleSlnx(slnxFile);
            if (solution != null)
                solutions.Add(solution);
        }
        return solutions;
    }

    static IEnumerable<FileInfo> FindSlnxFilesRecursively(DirectoryInfo directory)
    {
        foreach (var file in directory.GetFiles("*.slnx"))
            yield return file;

        foreach (var subdirectory in directory.GetDirectories())
        {
            if (DirectoriesToSkip.Contains(subdirectory.Name, StringComparer.OrdinalIgnoreCase))
                continue;

            foreach (var file in FindSlnxFilesRecursively(subdirectory))
                yield return file;
        }
    }

    static SlnxSolutionInfo? ParseSingleSlnx(FileInfo slnxFile)
    {
        try
        {
            var document = XDocument.Load(slnxFile.FullName);
            var projectFileNames = document.Descendants("Project")
                .Select(element => element.Attribute("Path")?.Value)
                .Where(path => path != null)
                .Select(path => Path.GetFileName(path!))
                .ToList();

            return new SlnxSolutionInfo(
                SlnxFullPath: slnxFile.FullName,
                ProjectFileNames: projectFileNames);
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine($"Warning: Could not parse {slnxFile.FullName}: {exception.Message}");
            return null;
        }
    }
}
