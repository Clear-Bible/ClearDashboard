using System;
using System.IO.Compression;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GZipMultiLib
{
    public class GZipFiles
    {
        //[FileNameLength 4bytes][FileName (FileNameLength bytes)][FileLength 8bytes][File (FileLength bytes)] .. repeat
        public static void Compress(string filepath, List<FileInfo> files)
        {
            try
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    foreach (FileInfo file in files)
                    {
                        byte[] filenameBytes = Encoding.Unicode.GetBytes(file.Name);
                        byte[] filenameLength = BitConverter.GetBytes(filenameBytes.Length);
                        var fstream = file.OpenRead();
                        long size = fstream.Length;
                        byte[] fileBytesLength = BitConverter.GetBytes(size);
                        stream.Write(filenameLength, 0, filenameLength.Length);
                        stream.Write(filenameBytes, 0, filenameBytes.Length);
                        stream.Write(fileBytesLength, 0, fileBytesLength.Length);
                        fstream.CopyTo(stream);
                    }
                    stream.Position = 0;

                    using (FileStream compressTo = File.Create(filepath))
                    {
                        using (GZipStream compression = new GZipStream(compressTo, CompressionMode.Compress))
                        {
                            stream.CopyTo(compression);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occurred while compressing files\n{e.ToString()}");
            }
        }

        public static void Decompress(string filepath, string outputfolder)
        {
            try
            {
                using (FileStream decompressFrom = File.OpenRead(filepath))
                {
                    using (GZipStream compression = new GZipStream(decompressFrom, CompressionMode.Decompress))
                    {
                        using (MemoryStream decomp = new MemoryStream())
                        {
                            compression.CopyTo(decomp);
                            decomp.Position = 0;
                            byte[] filenameLength = new byte[4];
                            byte[] fileBytesLength = new byte[8];
                            while (decomp.Position != decomp.Length)
                            {
                                decomp.Read(filenameLength, 0, filenameLength.Length);
                                int filenameInt = BitConverter.ToInt32(filenameLength, 0);

                                byte[] filenameBytes = new byte[filenameInt];
                                decomp.Read(filenameBytes, 0, filenameBytes.Length);
                                string filename = Encoding.Unicode.GetString(filenameBytes);

                                decomp.Read(fileBytesLength, 0, fileBytesLength.Length);
                                long filesizeInt = BitConverter.ToInt64(fileBytesLength, 0);

                                byte[] fileBytes = new byte[filesizeInt];
                                decomp.Read(fileBytes, 0, fileBytes.Length);

                                File.WriteAllBytes(Path.Combine(outputfolder, filename), fileBytes);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Could not decompress {filepath}:\n{e.ToString()}");
            }
        }

    }
}
