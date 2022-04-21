using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ClearDashboard.Wpf.Strings;
using Microsoft.Extensions.Logging;
using Serilog.Core;

namespace ClearDashboard.Wpf.Helpers
{
    public static class LocalizationStrings
    {
        public static string Get(string key, ILogger logger)
        {
            string localizedString;
            try
            {
                localizedString = Resources.ResourceManager.GetString(key, Thread.CurrentThread.CurrentUICulture);
            }
            catch (Exception e)
            {
                logger.LogCritical($"Localization string missing for key '{key}' {e.Message} {Thread.CurrentThread.CurrentUICulture.Name}");
                localizedString = key;
            }

            if (localizedString == null)
            {
                logger.LogCritical($"Localization string missing for key '{key}' {Thread.CurrentThread.CurrentUICulture.Name}");
                localizedString = key;
            }
            return localizedString;
        }
    }
}
