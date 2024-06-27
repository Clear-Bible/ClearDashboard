using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClearDashboard.Wpf.Application.Helpers
{
    public static class AbstractionsSettingsHelper
    {

        public static string GetGitUrl()
        {
            return Settings.Default.GitRootUrl;
        }

        public static void SaveGitUrl(string gitRootUrl)
        {
            Settings.Default.GitRootUrl = gitRootUrl;
            Settings.Default.Save();
        }

        public static bool GetEnabledAlignmentEditing()
        {
            return Settings.Default.IsAlignmentEditingEnabled;
        }

        public static void SaveEnabledAlignmentEditing(bool value)
        {
            Settings.Default.IsAlignmentEditingEnabled = value;
            Settings.Default.Save();
        }


        public static bool GetEnabledLexiconImport()
        {
            return Settings.Default.IsLexiconImportEnabled;
        }

        public static void SaveEnabledLexiconImport(bool value)
        {
            Settings.Default.IsLexiconImportEnabled = value;
            Settings.Default.Save();
        }

        public static bool GetShowExternalNotes()
        {
            return Settings.Default.ShowExternalNotes;
        }

        public static void SaveShowExternalNotes(bool value)
        {
            Settings.Default.ShowExternalNotes = value;
            Settings.Default.Save();
        }

        public static bool GetExternalNotesEnabled()
        {
            return Settings.Default.IsExternalNotesEnabled;
        }

        public static void SaveExternalNotesEnabled(bool value)
        {
            Settings.Default.IsExternalNotesEnabled = value;
            Settings.Default.Save();
        }
    }
}
