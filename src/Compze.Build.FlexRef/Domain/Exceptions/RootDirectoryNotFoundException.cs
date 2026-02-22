namespace Compze.Build.FlexRef.Domain.Exceptions;

class RootDirectoryNotFoundException(DirectoryInfo rootDirectory)
    : Exception($"Root directory not found: {rootDirectory.FullName}")
{
    public DirectoryInfo RootDirectory { get; } = rootDirectory;
}
