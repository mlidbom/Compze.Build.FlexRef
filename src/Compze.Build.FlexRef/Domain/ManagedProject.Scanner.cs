using Microsoft.Build.Evaluation;

namespace Compze.Build.FlexRef.Domain;

partial class ManagedProject
{
    static class Scanner
    {
        internal static List<ManagedProject> ScanDirectory(FlexRefWorkspace workspace)
        {
            using var projectCollection = new ProjectCollection();
            return FindCsprojFilesRecursively(workspace.RootDirectory)
                .Select(csprojFile => new ManagedProject(csprojFile, projectCollection, workspace))
                .ToList();
        }

        static IEnumerable<FileInfo> FindCsprojFilesRecursively(DirectoryInfo directory)
        {
            foreach(var file in directory.GetFiles(DomainConstants.CsprojSearchPattern))
                yield return file;

            foreach(var subdirectory in directory.GetDirectories())
            {
                if(DomainConstants.DirectoriesToSkip.Contains(subdirectory.Name, StringComparer.OrdinalIgnoreCase))
                    continue;

                foreach(var file in FindCsprojFilesRecursively(subdirectory))
                    yield return file;
            }
        }
    }
}
