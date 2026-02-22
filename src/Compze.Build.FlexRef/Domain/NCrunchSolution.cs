using System.Xml.Linq;
using Compze.Build.FlexRef.SystemCE.XmlCE.LinqCE;

namespace Compze.Build.FlexRef.Domain;

class NCrunchSolution
{
    readonly SlnxSolution _slnxSolution;
    FileInfo File => _slnxSolution.NCrunchFile;
    List<FlexReferencedProject> AbsentFlexReferencedProjects => _slnxSolution.AbsentFlexReferencedProjects;

    internal NCrunchSolution(SlnxSolution slnxSolution) =>
        _slnxSolution = slnxSolution;

    public void UpdateOrCreate()
    {
        if(File.Exists)
            Update();
        else
            Create();
    }

    void Create()
    {
        var settingsElement = new XElement("Settings");

        if(AbsentFlexReferencedProjects.Count > 0)
        {
            var customBuildProperties = new XElement("CustomBuildProperties");
            foreach(var flexReferencedProject in AbsentFlexReferencedProjects)
                customBuildProperties.Add(new XElement("Value", $"{flexReferencedProject.PropertyName} = true"));
            settingsElement.Add(customBuildProperties);
        }

        var document = new XDocument(
            new XElement("SolutionConfiguration", settingsElement));

        document.SaveWithoutDeclaration(File.FullName);
        Console.WriteLine($"  Created: {File.FullName} ({AbsentFlexReferencedProjects.Count} absent package(s))");
    }

    void Update()
    {
        var document = XDocument.Load(File.FullName);
        var rootElement = document.Root!;

        var settingsElement = rootElement.Element("Settings");
        if(settingsElement == null)
        {
            settingsElement = new XElement("Settings");
            rootElement.Add(settingsElement);
        }

        var customBuildProperties = settingsElement.Element("CustomBuildProperties");

        if(customBuildProperties != null)
        {
            var existingFlexRefValues = customBuildProperties.Elements("Value")
                .Where(value => value.Value.TrimStart().StartsWith(DomainConstants.UsePackageReferencePropertyPrefix))
                .ToList();

            foreach(var value in existingFlexRefValues)
                value.Remove();
        }

        if(AbsentFlexReferencedProjects.Count > 0)
        {
            if(customBuildProperties == null)
            {
                customBuildProperties = new XElement("CustomBuildProperties");
                settingsElement.Add(customBuildProperties);
            }

            foreach(var flexReferencedProject in AbsentFlexReferencedProjects)
                customBuildProperties.Add(new XElement("Value", $"{flexReferencedProject.PropertyName} = true"));
        }

        if(customBuildProperties is { HasElements: false })
            customBuildProperties.Remove();

        document.SaveWithoutDeclaration(File.FullName);
        Console.WriteLine($"  Updated: {File.FullName} ({AbsentFlexReferencedProjects.Count} absent package(s))");
    }
}
