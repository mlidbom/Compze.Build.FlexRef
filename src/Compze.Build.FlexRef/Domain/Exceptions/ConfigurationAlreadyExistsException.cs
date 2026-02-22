namespace Compze.Build.FlexRef.Domain.Exceptions;

class ConfigurationAlreadyExistsException(DirectoryInfo rootDirectory)
    : Exception($"Configuration already exists: {Path.Combine(rootDirectory.FullName, DomainConstants.ConfigurationFileName)}")
{
    public DirectoryInfo RootDirectory { get; } = rootDirectory;
}
