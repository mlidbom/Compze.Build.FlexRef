using Compze.Build.FlexRef.SystemCE.IOCE;
using Microsoft.Build.Evaluation;

namespace Compze.Build.FlexRef.Domain;

partial class ManagedProject
{
    static class Scanner
    {
        internal static List<ManagedProject> ScanDirectory(FlexRefWorkspace workspace)
        {
            using var projectCollection = new ProjectCollection();
            return workspace.RootDirectory
                .EnumerateFiles(DomainConstants.CsprojSearchPattern, SearchOption.AllDirectories)
                .Where(file => !DomainConstants.DirectoriesToSkip.Any(file.HasDirectoryInPath))
                .Select(csprojFile => new ManagedProject(csprojFile, projectCollection, workspace))
                .ToList();
        }
    }
}
