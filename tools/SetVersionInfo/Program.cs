// See https://aka.ms/new-console-template for more information
using System.Xml;
using System.Xml.Linq;

using System;

namespace MyApp // Note: actual namespace depends on the project name.
{
    internal class Program
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputVersionNumber"></param>
        static void Main(string inputVersionNumber = "")
        {
            string? versionNumber;
            if (string.IsNullOrEmpty(inputVersionNumber))
            {
                Console.WriteLine("Enter New Version Number (e.g., 1.0.6.11):");

                versionNumber = Console.ReadLine();
            }
            else
            {
                versionNumber = inputVersionNumber;
            }

#if DEBUG
            // Run from the SetVersionInfo bin\Debug folder
            DirectoryInfo dir = new DirectoryInfo(@"..\..\..\..\..\src\");
            DirectoryInfo installerDir = new DirectoryInfo(@"..\..\..\..\..\installer\");

#else
            // Run from the Installer folder
            DirectoryInfo dir = new DirectoryInfo(@"..\src\");
            DirectoryInfo installerDir = new DirectoryInfo(".");

#endif

            foreach (FileInfo file in installerDir.GetFiles("DashboardInstaller.iss", SearchOption.AllDirectories))
            {
                var installerScriptLinesArr = File.ReadAllLines(file.FullName);

                var versionLineIndex = 0;
                foreach (var line in installerScriptLinesArr)
                {
                    if (line.StartsWith("#define MyAppVersion"))
                    {
                        break;
                    }
                    versionLineIndex++;
                }

                installerScriptLinesArr[versionLineIndex] = $"#define MyAppVersion \"{versionNumber}\"";

                File.WriteAllLines(file.FullName, installerScriptLinesArr);
            }

            foreach (FileInfo file in dir.GetFiles("app.manifest", SearchOption.AllDirectories))
            {
                Console.WriteLine(file.FullName);

                XmlDocument doc = new XmlDocument();
                doc.Load(file.FullName);

                // Find the assemblyIdentity node using its namespace and local name
                XmlNamespaceManager namespaceManager = new XmlNamespaceManager(doc.NameTable);
                namespaceManager.AddNamespace("ns", "urn:schemas-microsoft-com:asm.v1");
                XmlNode assemblyIdentityNode = doc.SelectSingleNode("//ns:assemblyIdentity", namespaceManager);

                if (assemblyIdentityNode == null)
                {
                    continue;
                }

                // Update the value of the version attribute
                XmlAttribute versionAttribute = assemblyIdentityNode.Attributes["version"];
                versionAttribute.Value = versionNumber;

                // Save the modified XML document
                doc.Save(file.FullName);
            }

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
        }
    }
}





