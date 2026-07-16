namespace Compze.Build.FlexRef.Domain;

/// <summary>
/// A directory subtree that project and solution scanning must skip, named by a path relative to the
/// workspace root in an <c>&lt;ExcludeDirectory Path="..." /&gt;</c> config element.
/// </summary>
/// <remarks>
/// Exists so a repository can point FlexRef away from a location that duplicates projects already present
/// elsewhere in the tree — typically a directory junction or symbolic link that resolves back inside the
/// repository (for example the docs website's co-located-docs junctions). Without it, scanning discovers
/// those projects a second time and both mis-references them and rewrites their references. See
/// <see cref="FlexRefConfigurationFile.ExcludedDirectoryPaths"/> for the full rationale.
/// </remarks>
class ExcludedDirectory
{
   readonly DirectoryInfo _directory;

   public ExcludedDirectory(DirectoryInfo workspaceRoot, string relativePath) =>
      _directory = new DirectoryInfo(Path.Combine(workspaceRoot.FullName, relativePath));

   /// <summary>Whether <paramref name="file"/> lives anywhere beneath this excluded directory.</summary>
   public bool Contains(FileInfo file) =>
      file.FullName.StartsWith(_directory.FullName + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
}
