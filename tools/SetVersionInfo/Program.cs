// See https://aka.ms/new-console-template for more information
using System.Xml;
using System.Xml.Linq;

Console.WriteLine("Enter New Version Number (e.g., 1.0.6.11):");

string? versionNumber = Console.ReadLine();

#if DEBUG

DirectoryInfo dir = new DirectoryInfo(@"..\..\..\..\..\src\");

#else

DirectoryInfo dir = new DirectoryInfo(@"..\src\");

#endif


foreach (FileInfo file in dir.GetFiles("*.csproj", SearchOption.AllDirectories))
{
    Console.WriteLine(file.FullName);

    XmlDocument doc = new XmlDocument();
    doc.Load(file.FullName);

    XmlNode rootNode = doc.SelectSingleNode("//PropertyGroup");
    if (rootNode == null)
    {
        rootNode = doc.CreateElement("PropertyGroup");
        doc.AppendChild(rootNode);
    }


    XmlNode fileVersionNode = rootNode.SelectSingleNode("FileVersion");
    if (fileVersionNode == null)
    {
        fileVersionNode = doc.CreateElement("FileVersion");
        fileVersionNode.InnerText = versionNumber;
        rootNode.AppendChild(fileVersionNode);
    }
    else
    {
        fileVersionNode.InnerText = versionNumber;
    }

    XmlNode versionNode = rootNode.SelectSingleNode("Version");
    if (versionNode == null)
    {
        versionNode = doc.CreateElement("Version");
        versionNode.InnerText = versionNumber;
        rootNode.AppendChild(versionNode);
    }
    else
    {
        versionNode.InnerText = versionNumber;
    }

    XmlNode assemblyVersionNode = rootNode.SelectSingleNode("AssemblyVersion");
    if (assemblyVersionNode == null)
    {
        assemblyVersionNode = doc.CreateElement("AssemblyVersion");
        assemblyVersionNode.InnerText = versionNumber;
        rootNode.AppendChild(assemblyVersionNode);
    }
    else
    {
        assemblyVersionNode.InnerText = versionNumber;
    }

    doc.Save(file.FullName);

    Console.WriteLine($"Version Info set to: {versionNumber}");
}


