namespace Compze.Build.FlexRef.Cli;

record FlexReference(
    string PackageId,
    string CsprojFileName,
    string CsprojFullPath)
{
    public string PropertyName { get; } = "UsePackageReference_" + PackageId.Replace('.', '_').Replace('-', '_');
}
