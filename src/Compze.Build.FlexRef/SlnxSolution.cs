namespace Compze.Build.FlexRef;

partial class SlnxSolution
{
    public FileInfo SlnxFile { get; }
    public List<string> ProjectFileNames { get; }

    SlnxSolution(FileInfo slnxFile, List<string> projectFileNames)
    {
        SlnxFile = slnxFile;
        ProjectFileNames = projectFileNames;
    }

    public static List<SlnxSolution> FindAndParseAllSolutions(DirectoryInfo rootDirectory) =>
        Scanner.FindAndParseAll(rootDirectory);

    public void UpdateNCrunchFileIfNeeded(IReadOnlyList<FlexReference> flexReferences) =>
        NCrunchUpdater.UpdateOrCreate(this, flexReferences);

    public List<FlexReference> FindAbsentFlexReferences(IReadOnlyList<FlexReference> flexReferences) =>
        flexReferences
                      .Where(package => !ProjectFileNames
                                           .Contains(package.CsprojFile.Name, StringComparer.OrdinalIgnoreCase))
                      .OrderBy(package => package.PackageId, StringComparer.OrdinalIgnoreCase)
                      .ToList();
}
