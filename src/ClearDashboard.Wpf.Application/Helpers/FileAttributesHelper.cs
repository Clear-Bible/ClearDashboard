using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClearDashboard.Wpf.Application.Helpers
{
    public static class FileAttributesHelper
    {
        // recursively iterate through all the files in a directory and set their attributes to normal
        // so they can be deleted
        public static void SetNormalFileAttributes(string path)
        {
            var directoryInfo = new DirectoryInfo(path);
            foreach (var file in directoryInfo.GetFiles())
            {
                // take off the read-only attribute
                File.SetAttributes(file.FullName, FileAttributes.Normal);
            }
            foreach (var directory in directoryInfo.GetDirectories())
            {
                SetNormalFileAttributes(directory.FullName);
            }
        }

    }
}
