using ClearDashboard.Wpf.Application.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.Wpf.Application.Helpers
{
    public static class MySqlHelper
    {
        public static string BuildConnectionString()
        {
            var gitRootUrl = AbstractionsSettingsHelper.GetGitUrl();
            var uri = new Uri(gitRootUrl);

            var userId = Encryption.Decrypt("aOyUlhtlSFsdhxiq3HOgsg==");
            var pass = Encryption.Decrypt("9bf0Wjv4cwHDdgRkkBk84QkaSuwCppE3t6Mhpp+zYrY=");

            return $"Server={uri.Host};User ID={userId};Password={pass};Database=dashboard";
        }

    }
}
