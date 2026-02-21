namespace Compze.Build.FlexRef;

class FlexRefWorkspace
{
    public IReadOnlyList<ManagedProject> AllProjects { get; }
    public IReadOnlyList<FlexReference> FlexReferences { get; }

    internal FlexRefWorkspace(List<ManagedProject> allProjects, List<FlexReference> flexReferences)
    {
        AllProjects = allProjects;
        FlexReferences = flexReferences;
    }

    public static FlexRefWorkspace ScanAndResolve(DirectoryInfo rootDirectory, FlexRefConfigurationFile configuration)
    {
        var allProjects = ManagedProject.ScanDirectory(rootDirectory);
        var flexReferences = ManagedProject.ResolveFlexReferences(configuration, allProjects);
        return new FlexRefWorkspace(allProjects, flexReferences);
    }
}
