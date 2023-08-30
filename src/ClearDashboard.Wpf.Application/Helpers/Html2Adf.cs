using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace ClearDashboard.Wpf.Application.Helpers
{
    public static class Html2Adf
    {
        public static async Task<string> Convert(string html)
        {
            // get a temp filepath
            string tempFilePath = Path.GetTempFileName();
            await File.WriteAllTextAsync(tempFilePath, html);
            // get a temp output filepath
            string tempOutputFilePath = Path.GetTempFileName();

            // get the path to the html2adf.exe
            string filePath = Path.Combine(Environment.CurrentDirectory, @"resources\html2adf\html2adf.exe");

            if (File.Exists(filePath))
            {
                // run an external program and wait for it to exit
                Process process = new Process();
                process.StartInfo.FileName = filePath;
                process.StartInfo.Arguments = $"{tempFilePath} -o {tempOutputFilePath}";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.Start();

                // Wait for the process to finish
                await process.WaitForExitAsync();
            }
            else
            {
                CleanUp(tempFilePath, tempOutputFilePath);
                return "";
            }


            // read the output file
            if (File.Exists(tempOutputFilePath))
            {
                string adf = File.ReadAllText(tempOutputFilePath);

                CleanUp(tempFilePath, tempOutputFilePath);
                return adf;
            }

            CleanUp(tempFilePath, tempOutputFilePath);
            return "";
        }


        public static void CleanUp(string path1, string path2)
        {
            try
            {
                File.Delete(path1);
                File.Delete(path2);
            }
            catch (Exception)
            {
            }
        }

    }
}
