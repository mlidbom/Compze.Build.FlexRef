namespace Compze.Build.FlexRef;

class FlexRefWorkspace
{
   public DirectoryInfo RootDirectory { get; }
   public IReadOnlyList<ManagedProject> AllProjects { get; }
   public IReadOnlyList<FlexReference> FlexReferences { get; }

   internal FlexRefWorkspace(DirectoryInfo rootDirectory, List<ManagedProject> allProjects, List<FlexReference> flexReferences)
   {
      RootDirectory = rootDirectory;
      AllProjects = allProjects;
      FlexReferences = flexReferences;
   }

   public static FlexRefWorkspace ScanAndResolve(DirectoryInfo rootDirectory, FlexRefConfigurationFile configuration)
   {
      var allProjects = ManagedProject.ScanDirectory(rootDirectory);
      var flexReferences = ManagedProject.ResolveFlexReferences(configuration, allProjects);
      return new FlexRefWorkspace(rootDirectory, allProjects, flexReferences);
   }

   public void UpdateDirectoryBuildProps() => DirectoryBuildPropsFileUpdater.UpdateOrCreate(this);

   public void UpdateCsprojFiles() => new CsprojUpdater(this).UpdateAll();

   public void UpdateNCrunchFiles() => new NCrunchUpdater(this).UpdateAll();
}
