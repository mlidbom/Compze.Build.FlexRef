namespace Compze.Build.FlexRef.Domain;

partial class SlnxSolution
{
    static class Scanner
    {
        public static List<SlnxSolution> FindAndParseAll(FlexRefWorkspace workspace) =>
            FindSlnxFilesRecursively(workspace.RootDirectory)
               .Select(slnxFile => new SlnxSolution(slnxFile, workspace))
               .ToList();

        static IEnumerable<FileInfo> FindSlnxFilesRecursively(DirectoryInfo directory)
        {
            foreach(var file in directory.GetFiles(DomainConstants.SlnxSearchPattern))
                yield return file;

            foreach(var subdirectory in directory.GetDirectories())
            {
                if(DomainConstants.DirectoriesToSkip.Contains(subdirectory.Name, StringComparer.OrdinalIgnoreCase))
                    continue;

                foreach(var file in FindSlnxFilesRecursively(subdirectory))
                    yield return file;
            }
        }
    }
}
