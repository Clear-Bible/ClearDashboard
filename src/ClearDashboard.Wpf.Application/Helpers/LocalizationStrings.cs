using System;
using System.Threading;
using ClearDashboard.Wpf.Application.Strings;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.Wpf.Application.Helpers
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
