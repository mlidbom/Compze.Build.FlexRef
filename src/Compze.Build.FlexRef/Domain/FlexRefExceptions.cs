namespace Compze.Build.FlexRef.Domain;

class ConfigurationAlreadyExistsException(DirectoryInfo rootDirectory)
    : Exception($"Configuration already exists: {Path.Combine(rootDirectory.FullName, "FlexRef.config.xml")}")
{
    public DirectoryInfo RootDirectory { get; } = rootDirectory;
}

class ConfigurationNotFoundException(DirectoryInfo rootDirectory)
    : Exception($"Configuration not found: {Path.Combine(rootDirectory.FullName, "FlexRef.config.xml")}")
{
    public DirectoryInfo RootDirectory { get; } = rootDirectory;
}
