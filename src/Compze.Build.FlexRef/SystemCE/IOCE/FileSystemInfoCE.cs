namespace Compze.Build.FlexRef.SystemCE.IOCE;

static class FileSystemInfoCE
{
    public static bool HasDirectoryInPath(this FileSystemInfo fileSystemInfo, string directoryName) =>
        fileSystemInfo.FullName.Contains(Path.DirectorySeparatorChar + directoryName + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
}
