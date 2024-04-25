using System;
using System.IO;
using System.Xml;

namespace CompressXML
{
    static class Program
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="DirectoryPath"></param>
        static void Main(string DirectoryPath = @"D:\Projects-GBI\ClearEngine3\test\TestSandbox1\Resources\treebank\Clear3Dev", bool Decompress = false)
        {
            var currentDirectory = Directory.GetCurrentDirectory();

            DirectoryPath = Path.Combine(currentDirectory, "..", @"src\ClearDashboard.Wpf.Application\bin\Release\net8.0-windows\publish\win-x64\Resources");

            if (Decompress)
            {
                string sNewDirectoryPath = Path.Combine(DirectoryPath, "compressed");
                string filePath = Path.Combine(sNewDirectoryPath, "gzipped", "trees.gzip");

                if (! Directory.Exists(sNewDirectoryPath))
                {
                    Console.WriteLine("Output Directory does not exist: " + sNewDirectoryPath);
                    return;
                }

                if (! File.Exists(filePath))
                {
                    Console.WriteLine("Input gzip file does not exist: " + filePath);
                    return;
                }

                GZipMultiLib.GZipFiles.Decompress(filePath, DirectoryPath);
            }
            else
            {
                if (!Directory.Exists(DirectoryPath))
                {
                    Console.WriteLine("Directory Path does not exist: " + DirectoryPath);
                    return;
                }
                
                var searchOptions = SearchOption.AllDirectories;
                string[] sFiles = Directory.GetFiles(DirectoryPath, "*.xml", searchOptions);

                // strip out the extra verbose whitespace
                foreach (var sFile in sFiles)
                {
                    var newFile = CompressXML(sFile);

                    File.Delete(sFile);
                    File.Move(newFile, sFile);
                }

                string[] compressedDirectories = Directory.GetDirectories(DirectoryPath, "compressed", searchOptions);
                foreach (var directory in compressedDirectories)
                {
                    Directory.Delete(directory);
                }
            }
        }


        public static string CompressXML(string sFilePath)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(sFilePath);

            FileInfo fi = new FileInfo(sFilePath);

            string sNewDirectoryPath = Path.Combine(fi.Directory.FullName, "compressed");
            if (!Directory.Exists(sNewDirectoryPath))
            {
                Directory.CreateDirectory(sNewDirectoryPath);
            }

            string newPath = Path.Combine(fi.DirectoryName, "compressed", fi.Name);

            File.WriteAllText(newPath, doc.OuterXml);


            FileInfo fiNew = new FileInfo(newPath);

            Console.WriteLine($"{fi.Name} {fi.Length}");
            Console.WriteLine($"{fiNew.Name} {fiNew.Length}");
            Console.WriteLine("-------");

            return newPath;
        }
    }
}
