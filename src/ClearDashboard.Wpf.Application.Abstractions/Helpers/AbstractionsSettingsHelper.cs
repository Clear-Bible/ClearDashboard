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

    }
}
