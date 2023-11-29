using ClearDashboard.DataAccessLayer.Models;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClearDashboard.WebApiParatextPlugin.Helpers
{
    public static class ParatextHelpers
    {
        /// <summary>
        /// Returns the directory path to where the Paratext project resides
        /// </summary>
        /// <returns></returns>
        public static string GetParatextProjectsPath()
        {
            string paratextProjectPath = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Paratext\8", "Settings_Directory", null);
            // check if directory exists
            if (!Directory.Exists(paratextProjectPath))
            {
                // directory doesn't exist so null this out
                paratextProjectPath = "";
            }

            return paratextProjectPath;
        }



        public static List<ParatextBook> GetUsfmFilePaths()
        {
            return new List<ParatextBook>();
        }

    }
}
