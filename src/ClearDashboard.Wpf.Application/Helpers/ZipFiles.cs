using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace ClearDashboard.Wpf.Application.Helpers
{
    /// <summary>
    /// Zip up a set of files using the .NET ZipFile class
    /// </summary>
    public class ZipFiles
    {
        public List<string> Files { get; }
        public string ZipFilePath { get; }


        public ZipFiles(List<string> files, string zipFilePath)
        {
            Files = files;
            ZipFilePath = zipFilePath;
        }

        public bool Zip()
        {
            try
            {
                using (MemoryStream zipMS = new MemoryStream())
                {
                    using (ZipArchive zipArchive = new ZipArchive(zipMS, ZipArchiveMode.Create, true))
                    {
                        //loop through files to add
                        foreach (string fileToZip in Files)
                        {
                            //read the file bytes
                            byte[] fileToZipBytes = File.ReadAllBytes(fileToZip);

                            //create the entry - this is the zipped filename
                            FileInfo fi = new FileInfo(fileToZip);

                            ZipArchiveEntry zipFileEntry = zipArchive.CreateEntry(fi.Name);

                            //add the file contents
                            using (Stream zipEntryStream = zipFileEntry.Open())
                            using (BinaryWriter zipFileBinary = new BinaryWriter(zipEntryStream))
                            {
                                zipFileBinary.Write(fileToZipBytes);
                            }
                        }
                    }

                    using (FileStream finalZipFileStream = new FileStream(ZipFilePath, FileMode.Create))
                    {
                        zipMS.Seek(0, SeekOrigin.Begin);
                        zipMS.CopyTo(finalZipFileStream);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }

        }

    }
}
