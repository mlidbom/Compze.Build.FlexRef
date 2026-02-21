namespace Compze.Build.FlexRef.Cli;

record FlexReference(
    string PackageId,
    FileInfo CsprojFile)
{
    public string PropertyName { get; } = "UsePackageReference_" + PackageId.Replace('.', '_').Replace('-', '_');
}
