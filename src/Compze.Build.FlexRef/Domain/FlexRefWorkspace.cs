using Compze.Build.FlexRef.Domain.Exceptions;
using Compze.Build.FlexRef.SystemCE.IOCE;

namespace Compze.Build.FlexRef.Domain;

class FlexRefWorkspace
{
   public DirectoryInfo RootDirectory { get; }
   internal IReadOnlyList<CSProj> AllProjects { get; private set; } = [];
   internal IReadOnlyList<FlexReferencedProject> FlexReferencedProjects { get; private set; } = [];

   internal FlexRefConfigurationFile ConfigurationFile { get; }
   internal DirectoryBuildPropsFile DirectoryBuildPropsFile { get; }

   public FlexRefWorkspace(DirectoryInfo rootDirectory)
   {
      if(!rootDirectory.Exists)
         throw new RootDirectoryNotFoundException(rootDirectory);

      RootDirectory = rootDirectory;
      ConfigurationFile = new FlexRefConfigurationFile(this);
      DirectoryBuildPropsFile = new DirectoryBuildPropsFile(this);
   }

   /// <summary>
   /// Every file matching <paramref name="searchPattern"/> anywhere under <see cref="RootDirectory"/> that
   /// scanning should consider — build output and tooling folders skipped, NCrunch temp files skipped, and
   /// any directory the config lists via <see cref="FlexRefConfigurationFile.ExcludedDirectoryPaths"/> left
   /// out. Both the project scan and the solution scan share this one definition of what is in scope.
   /// </summary>
   internal IEnumerable<FileInfo> EnumerateScannableFiles(string searchPattern)
   {
      var excludedDirectories = ConfigurationFile.ExcludedDirectoryPaths
                                                 .Select(relativePath => new ExcludedDirectory(RootDirectory, relativePath))
                                                 .ToList();

      return RootDirectory
            .EnumerateFiles(searchPattern, SearchOption.AllDirectories)
            .Where(file => !DomainConstants.DirectoriesToSkip.Any(file.HasDirectoryInPath))
            .Where(file => !DomainConstants.FilenamePrefixesToSkip.Any(prefix => file.Name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
            .Where(file => !excludedDirectories.Any(excludedDirectory => excludedDirectory.Contains(file)));
   }

   void LoadConfiguration()
   {
      if(!ConfigurationFile.Exists())
         throw new ConfigurationNotFoundException(RootDirectory);

      ConfigurationFile.Load();
   }

   void ScanProjects() => AllProjects = CSProj.ScanDirectory(this);

   void ResolveFlexReferencedProjects() => FlexReferencedProjects = CSProj.ResolveFlexReferencedProjects(this);

   public void Init()
   {
      ScanProjects();

      if(ConfigurationFile.Exists())
         throw new ConfigurationAlreadyExistsException(RootDirectory);

      ConfigurationFile.CreateDefault();
      FlexRefPropsFile.Write(this);
   }

   public void Sync()
   {
      LoadConfiguration();
      ScanProjects();
      ResolveFlexReferencedProjects();

      FlexRefPropsFile.Write(this);
      DirectoryBuildPropsFile.UpdateOrCreate();
      CSProj.UpdateAll(this);

      foreach(var solution in SlnxSolution.FindAndParseAllSolutions(this))
         solution.UpdateNCrunchFile();
   }
}
