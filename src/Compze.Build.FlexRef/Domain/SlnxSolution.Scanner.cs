using Compze.Build.FlexRef.SystemCE.IOCE;

namespace Compze.Build.FlexRef.Domain;

partial class SlnxSolution
{
    static class Scanner
    {
        public static List<SlnxSolution> FindAndParseAll(FlexRefWorkspace workspace) =>
            workspace.RootDirectory
               .EnumerateFiles(DomainConstants.SlnxSearchPattern, SearchOption.AllDirectories)
               .Where(file => !DomainConstants.DirectoriesToSkip.Any(file.HasDirectoryInPath))
               .Select(slnxFile => new SlnxSolution(slnxFile, workspace))
               .ToList();
    }
}
