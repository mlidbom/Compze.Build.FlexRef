using System.Xml.Linq;

namespace Compze.Build.FlexRef.Cli;

static class CsprojFileUpdater
{
    public static void UpdateIfNeeded(DiscoveredProject project, List<FlexReference> switchablePackages)
    {
        var referencedSwitchablePackages = DetermineReferencedSwitchablePackages(project, switchablePackages);

        if (referencedSwitchablePackages.Count == 0)
            return;

        var document = XDocument.Load(project.CsprojFile.FullName);
        var rootElement = document.Root!;

        RemoveExistingSwitchableReferences(rootElement, switchablePackages);
        AppendSwitchableReferencePairs(rootElement, project.CsprojFile, referencedSwitchablePackages);

        XmlFileHelper.SaveWithoutDeclaration(document, project.CsprojFile.FullName);
        Console.WriteLine($"  Updated: {project.CsprojFile.FullName} ({referencedSwitchablePackages.Count} switchable reference(s))");
    }

    static List<FlexReference> DetermineReferencedSwitchablePackages(
        DiscoveredProject project,
        List<FlexReference> switchablePackages)
    {
        var result = new List<FlexReference>();

        foreach (var package in switchablePackages)
        {
            if (project.CsprojFile.FullName.Equals(package.CsprojFile.FullName, StringComparison.OrdinalIgnoreCase))
                continue;

            var hasMatchingProjectReference = project.ProjectReferences
                .Any(reference => reference.ResolvedFileName.Equals(package.CsprojFile.Name, StringComparison.OrdinalIgnoreCase));

            var hasMatchingPackageReference = project.PackageReferences
                .Any(reference => reference.PackageName.Equals(package.PackageId, StringComparison.OrdinalIgnoreCase));

            if (hasMatchingProjectReference || hasMatchingPackageReference)
                result.Add(package);
        }

        return result.OrderBy(package => package.PackageId, StringComparer.OrdinalIgnoreCase).ToList();
    }

    static void RemoveExistingSwitchableReferences(XElement rootElement, List<FlexReference> switchablePackages)
    {
        // First pass: remove entire ItemGroups conditioned on UsePackageReference_* for switchable packages
        var conditionalItemGroups = rootElement.Elements("ItemGroup")
            .Where(itemGroup =>
            {
                var condition = itemGroup.Attribute("Condition")?.Value ?? "";
                return switchablePackages.Any(package => condition.Contains(package.PropertyName));
            })
            .ToList();

        foreach (var itemGroup in conditionalItemGroups)
            RemoveElementAndPrecedingComment(itemGroup);

        // Second pass: remove individual switchable references from remaining (unconditional) ItemGroups
        foreach (var itemGroup in rootElement.Elements("ItemGroup").ToList())
        {
            var referencesToRemove = itemGroup.Elements()
                .Where(element => IsReferenceToSwitchablePackage(element, switchablePackages))
                .ToList();

            foreach (var reference in referencesToRemove)
                reference.Remove();

            if (!itemGroup.HasElements)
                RemoveElementAndPrecedingComment(itemGroup);
        }
    }

    static bool IsReferenceToSwitchablePackage(XElement element, List<FlexReference> switchablePackages)
    {
        if (element.Name.LocalName == "PackageReference")
        {
            var includeName = element.Attribute("Include")?.Value;
            return includeName != null &&
                switchablePackages.Any(package =>
                    package.PackageId.Equals(includeName, StringComparison.OrdinalIgnoreCase));
        }

        if (element.Name.LocalName == "ProjectReference")
        {
            var includePath = element.Attribute("Include")?.Value;
            if (includePath == null) return false;
            var fileName = Path.GetFileName(includePath);
            return switchablePackages.Any(package =>
                package.CsprojFile.Name.Equals(fileName, StringComparison.OrdinalIgnoreCase));
        }

        return false;
    }

    static void AppendSwitchableReferencePairs(
        XElement rootElement,
        FileInfo consumingCsprojFile,
        List<FlexReference> referencedPackages)
    {
        foreach (var package in referencedPackages)
        {
            var relativeProjectPath = ComputeRelativePathWithBackslashes(
                consumingCsprojFile.FullName, package.CsprojFile.FullName);

            rootElement.Add(
                new XComment($" {package.PackageId} — switchable reference "),
                new XElement("ItemGroup",
                    new XAttribute("Condition", $"'$({package.PropertyName})' == 'true'"),
                    new XElement("PackageReference",
                        new XAttribute("Include", package.PackageId),
                        new XAttribute("Version", "*-*"))),
                new XElement("ItemGroup",
                    new XAttribute("Condition", $"'$({package.PropertyName})' != 'true'"),
                    new XElement("ProjectReference",
                        new XAttribute("Include", relativeProjectPath))));
        }
    }

    static string ComputeRelativePathWithBackslashes(string fromCsprojFullPath, string toCsprojFullPath)
    {
        var fromDirectory = Path.GetDirectoryName(fromCsprojFullPath)!;
        var relativePath = Path.GetRelativePath(fromDirectory, toCsprojFullPath);
        return relativePath.Replace('/', '\\');
    }

    static void RemoveElementAndPrecedingComment(XNode node)
    {
        if (node.PreviousNode is XComment)
            node.PreviousNode.Remove();
        node.Remove();
    }
}
