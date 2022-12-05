using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.Wpf.Application.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;

namespace ClearDashboard.Wpf.Application.Models
{
    public enum ReleaseNoteType
    {
        [Description("Added")]
        Added,
        [Description("Bug Fix")]
        BugFix,
        [Description("Changed")]
        Changed,
        [Description("Updated")]
        Updated,
        [Description("Deferred")]
        Deferred,
        [Description("Breaking Change")]
        BreakingChange
    }

    public class UpdateFormat
    {
        public string Version { get; set; } = String.Empty;
        public string ReleaseDate { get; set; } = String.Empty;
        public List<ReleaseNote> ReleaseNotes { get; set; } = new();
        public string DownloadLink { get; set; } = String.Empty;
    }

    public class ReleaseNote
    {
        public ReleaseNoteType NoteType { get; set; } = ReleaseNoteType.Added;
        public string Note { get; set; } = String.Empty;
        public string VersionNumber { get; set; } = String.Empty;
    }

    public static class ReleaseNotesManager
    {
        public static List<ReleaseNote> UpdateNotes { get; set; }
        public static List<UpdateFormat> UpdateData { get; set; }
        public static bool UpdateDataUpdated { get; set; }

        public static async Task<List<UpdateFormat>> GetUpdateDataFromFile()
        {
            if (!UpdateDataUpdated)
            {
                var connectedToInternet = await NetworkHelper.IsConnectedToInternet();
                if (!connectedToInternet)
                {
                    return UpdateData;
                }

                try
                {
                    HttpWebRequest req = (HttpWebRequest)WebRequest.Create("https://raw.githubusercontent.com/Clear-Bible/CLEAR_External_Releases/Version-0.4.4.0/VersionHistory/ClearDashboard.json");
                    req.UserAgent = "[any words that is more than 5 characters]";
                    req.Accept = "application/json";
                    WebResponse response = req.GetResponse(); //Error Here
                    Stream dataStream = response.GetResponseStream();
                    
                    var options = new JsonSerializerOptions { WriteIndented = true };
                    UpdateData = await JsonSerializer.DeserializeAsync<List<UpdateFormat>>(dataStream, options);
                    UpdateDataUpdated = true;
                }
                catch (Exception)
                {
                    return UpdateData;
                }
            }
            return UpdateData;
        }

        public static async Task<List<ReleaseNote>> GetUpdateNotes(List<UpdateFormat> updateDataList)
        {
            var isNewer = CheckWebVersion(updateDataList.LastOrDefault().Version);

            if (isNewer)
            {
                var combinedReleaseNotes = new List<ReleaseNote>();
                foreach (var update in updateDataList)
                {
                    if (CheckWebVersion(update.Version))
                    {
                        combinedReleaseNotes.AddRange(update.ReleaseNotes);
                    }
                }

                UpdateNotes = combinedReleaseNotes;
                return UpdateNotes;
            }

            return UpdateNotes;
        }

        public static async Task<List<ReleaseNote>> GetUpdateNotes()
        {
            var updateDataList = await GetUpdateDataFromFile();
            
            UpdateNotes = await GetUpdateNotes(updateDataList);

            return UpdateNotes;
        }

        public static async Task<List<UpdateFormat>> GetRelevantUpdates(List<UpdateFormat> updateDataList)
        {
            var selectUpdates = new List<UpdateFormat>();
            foreach (var update in updateDataList)
            {
                //if (CheckWebVersion(update.Version))
                //{
                    selectUpdates.Add(update);
                //}
            }

            UpdateData = selectUpdates;
            return UpdateData;
        }

        public static bool CheckWebVersion(string webVersion)
        {
            var webVer = ParseVersionString(webVersion);

            //get the assembly version
            var thisVersion = Assembly.GetEntryAssembly().GetName().Version;

            return IsNewerVersion(thisVersion, webVer);
        }

        private static Version ParseVersionString(string webVersion)
        {
            //convert string to version
            var ver = webVersion.Split('.');
            Version webVer;

            switch (ver.Length)
            {
                case 4:
                    try
                    {
                        webVer = new Version(Convert.ToInt32(ver[0]), Convert.ToInt32(ver[1]), Convert.ToInt32(ver[2]), Convert.ToInt32(ver[3]));
                    }
                    catch (Exception)
                    {
                        return null;
                    }

                    break;
                case 3:
                    try
                    {
                        webVer = new Version(Convert.ToInt32(ver[0]), Convert.ToInt32(ver[1]), Convert.ToInt32(ver[2]), 0);
                    }
                    catch (Exception)
                    {
                        return null;
                    }

                    break;
                case 2:
                    try
                    {
                        webVer = new Version(Convert.ToInt32(ver[0]), Convert.ToInt32(ver[1]), 0, 0);
                    }
                    catch (Exception)
                    {
                        return null;
                    }

                    break;
                default:
                {
                    if (ver.Length == 2)
                    {
                        try
                        {
                            webVer = new Version(Convert.ToInt32(ver[0]), 0, 0, 0);
                        }
                        catch (Exception)
                        {
                            return null;
                        }
                    }
                    else
                    {
                        return null;
                    }

                    break;
                }
            }
            return webVer;
        }

        public static bool IsNewerVersion(Version olderVersion, Version newerVersion)
        {
            if (newerVersion.CompareTo(olderVersion) == 1)
            {
                return true;
            }
            return false;
        }

        public static async Task<bool> CheckVersionCompatibility(string projectVersion)
        {
            if (string.IsNullOrEmpty(projectVersion))
            {
                return false;
            }

            var updateData = await GetUpdateDataFromFile();
            var parsedProjectVersion = ParseVersionString(projectVersion);
            if (parsedProjectVersion == null)
            {
                return false;
            }
            return !IsBreakingChangePresent(updateData, parsedProjectVersion);
        }

        public static bool IsBreakingChangePresent(List<UpdateFormat> updateDataList, Version projectVersion)
        {
            foreach (var update in updateDataList)
            {
                var updateVersion = ParseVersionString(update.Version);
                if (!CheckWebVersion(update.Version) && IsNewerVersion(projectVersion, updateVersion))
                {
                    foreach (var releaseNote in update.ReleaseNotes)
                    {
                        if (releaseNote.NoteType == ReleaseNoteType.BreakingChange)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }
    }
}
