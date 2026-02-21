using Microsoft.Build.Evaluation;

namespace Compze.Build.FlexRef.Cli;

record DiscoveredProjectReference(string IncludePath, string ResolvedFileName);

record DiscoveredPackageReference(string PackageName, string Version);

class DiscoveredProject
{
    public FileInfo CsprojFile { get; }
    public string? PackageId { get; }
    public bool IsPackable { get; }
    public List<DiscoveredProjectReference> ProjectReferences { get; }
    public List<DiscoveredPackageReference> PackageReferences { get; }

    public DiscoveredProject(FileInfo csprojFile, ProjectCollection projectCollection)
    {
        CsprojFile = csprojFile;

        var msbuildProject = new Project(csprojFile.FullName, null, null, projectCollection);

        var explicitPackageId = msbuildProject.GetNonEmptyPropertyOrNull("PackageId");
        var isPackableValue = msbuildProject.GetNonEmptyPropertyOrNull("IsPackable");
        var isExplicitlyNotPackable = string.Equals(isPackableValue, "false", StringComparison.OrdinalIgnoreCase);
        var isExplicitlyPackable = string.Equals(isPackableValue, "true", StringComparison.OrdinalIgnoreCase);
        IsPackable = !isExplicitlyNotPackable && (explicitPackageId != null || isExplicitlyPackable);

        PackageId = explicitPackageId
            ?? (IsPackable ? Path.GetFileNameWithoutExtension(csprojFile.Name) : null);

        ProjectReferences = msbuildProject.GetItems("ProjectReference")
            .Select(item => item.EvaluatedInclude)
            .Where(includePath => !string.IsNullOrEmpty(includePath))
            .Select(includePath => new DiscoveredProjectReference(includePath, Path.GetFileName(includePath)))
            .ToList();

        PackageReferences = msbuildProject.GetItems("PackageReference")
            .Select(item => (Name: item.EvaluatedInclude, Version: item.GetMetadataValue("Version")))
            .Where(pair => !string.IsNullOrEmpty(pair.Name))
            .Select(pair => new DiscoveredPackageReference(pair.Name, pair.Version))
            .ToList();
    }
}
