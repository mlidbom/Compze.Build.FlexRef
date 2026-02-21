using System.Reflection;

namespace Compze.Build.FlexRef.Cli;

static class FlexRefPropsFileWriter
{
    const string BuildDirectoryName = "build";
    const string PropsFileName = "FlexRef.props";

    public static FileInfo GetPropsFile(DirectoryInfo rootDirectory) =>
        new(Path.Combine(rootDirectory.FullName, BuildDirectoryName, PropsFileName));

    public static string GetMsBuildImportProjectValue() =>
        $"$(MSBuildThisFileDirectory){BuildDirectoryName}\\{PropsFileName}";

    public static void WriteToDirectory(DirectoryInfo rootDirectory)
    {
        var targetFile = GetPropsFile(rootDirectory);
        Directory.CreateDirectory(targetFile.DirectoryName!);

        using var resourceStream = Assembly.GetExecutingAssembly()
            .GetManifestResourceStream("FlexRef.props")
            ?? throw new InvalidOperationException(
                "Embedded FlexRef.props resource not found in CLI assembly. This is a bug — please report it.");

        using var fileStream = File.Create(targetFile.FullName);
        resourceStream.CopyTo(fileStream);

        Console.WriteLine($"  Wrote: {targetFile.FullName}");
    }
}
