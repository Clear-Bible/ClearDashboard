using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

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

                string[] sFiles = Directory.GetFiles(DirectoryPath, "*.xml");

                // strip out the extra verbose whitespace
                foreach (var sFile in sFiles)
                {
                    CompressXML(sFile);
                }

                // prepare for gzipping all of these
                string sNewDirectoryPath = Path.Combine(DirectoryPath, "compressed");

                List<FileInfo> fiFiles = new List<FileInfo>();
                sFiles = Directory.GetFiles(sNewDirectoryPath, "*.xml");
                foreach (var sFile in sFiles)
                {
                    fiFiles.Add(new FileInfo(sFile));
                }

                // compress all the files into one gzip
                GZipMultiLib.GZipFiles.Compress(Path.Combine(sNewDirectoryPath, "gzipped", "trees.gzip"), fiFiles);
            }
        }


        public static void CompressXML(string sFilePath)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(sFilePath);

            FileInfo fi = new FileInfo(sFilePath);
            string newPath = Path.Combine(fi.DirectoryName, "compressed", fi.Name);

            File.WriteAllText(newPath, doc.OuterXml);


            FileInfo fiNew = new FileInfo(newPath);

            Console.WriteLine($"{fi.Name} {fi.Length}");
            Console.WriteLine($"{fiNew.Name} {fiNew.Length}");
            Console.WriteLine("-------");
        }
    }
}
