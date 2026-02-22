using System.Xml.Linq;
using Compze.Build.FlexRef.SystemCE.XmlCE.LinqCE;

namespace Compze.Build.FlexRef.Domain;

class NCrunchSolution
{
    readonly FileInfo _file;
    readonly List<FlexReferencedProject> _absentFlexReferencedProjects;

    internal NCrunchSolution(FileInfo file, List<FlexReferencedProject> absentFlexReferencedProjects)
    {
        _file = file;
        _absentFlexReferencedProjects = absentFlexReferencedProjects;
    }

    public void UpdateOrCreate()
    {
        if(_file.Exists)
            Update();
        else
            Create();
    }

    void Create()
    {
        var settingsElement = new XElement("Settings");

        if(_absentFlexReferencedProjects.Count > 0)
        {
            var customBuildProperties = new XElement("CustomBuildProperties");
            foreach(var flexReferencedProject in _absentFlexReferencedProjects)
                customBuildProperties.Add(new XElement("Value", $"{flexReferencedProject.PropertyName} = true"));
            settingsElement.Add(customBuildProperties);
        }

        var document = new XDocument(
            new XElement("SolutionConfiguration", settingsElement));

        document.SaveWithoutDeclaration(_file.FullName);
        Console.WriteLine($"  Created: {_file.FullName} ({_absentFlexReferencedProjects.Count} absent package(s))");
    }

    void Update()
    {
        var document = XDocument.Load(_file.FullName);
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

        if(_absentFlexReferencedProjects.Count > 0)
        {
            if(customBuildProperties == null)
            {
                customBuildProperties = new XElement("CustomBuildProperties");
                settingsElement.Add(customBuildProperties);
            }

            foreach(var flexReferencedProject in _absentFlexReferencedProjects)
                customBuildProperties.Add(new XElement("Value", $"{flexReferencedProject.PropertyName} = true"));
        }

        if(customBuildProperties is { HasElements: false })
            customBuildProperties.Remove();

        document.SaveWithoutDeclaration(_file.FullName);
        Console.WriteLine($"  Updated: {_file.FullName} ({_absentFlexReferencedProjects.Count} absent package(s))");
    }
}
