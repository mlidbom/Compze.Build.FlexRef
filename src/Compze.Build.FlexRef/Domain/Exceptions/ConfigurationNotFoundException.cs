namespace Compze.Build.FlexRef.Domain.Exceptions;

class ConfigurationNotFoundException(DirectoryInfo rootDirectory)
    : Exception($"Configuration not found: {Path.Combine(rootDirectory.FullName, DomainConstants.ConfigurationFileName)}")
{
    public DirectoryInfo RootDirectory { get; } = rootDirectory;
}
