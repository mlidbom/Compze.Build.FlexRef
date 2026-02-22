using System.Reflection;

namespace Compze.Build.FlexRef.Domain;

static class FlexRefPropsFileWriter
{
    public static FileInfo GetPropsFile(DirectoryInfo rootDirectory) =>
        new(Path.Combine(rootDirectory.FullName, DomainConstants.BuildDirectoryName, DomainConstants.PropsFileName));

    public static string GetMsBuildImportProjectValue() =>
        $"$(MSBuildThisFileDirectory){DomainConstants.BuildDirectoryName}\\{DomainConstants.PropsFileName}";

    public static void Write(FlexRefWorkspace workspace)
    {
        var targetFile = GetPropsFile(workspace.RootDirectory);
        Directory.CreateDirectory(targetFile.DirectoryName!);

        using var resourceStream = Assembly.GetExecutingAssembly()
            .GetManifestResourceStream(DomainConstants.PropsFileName)
            ?? throw new InvalidOperationException(
                "Embedded FlexRef.props resource not found in CLI assembly. This is a bug — please report it.");

        using var fileStream = File.Create(targetFile.FullName);
        resourceStream.CopyTo(fileStream);

        Console.WriteLine($"  Wrote: {targetFile.FullName}");
    }
}
