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
    public static class GetLocalizationString
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
                logger.LogCritical($"Localization missing for {key} {e.Message} {Thread.CurrentThread.CurrentUICulture.Name}");
                localizedString = key;
            }

            if (localizedString == null)
            {
                logger.LogCritical($"Localization missing for {key} {Thread.CurrentThread.CurrentUICulture.Name}");
                localizedString = key;
            }
            return localizedString;
        }
    }
}
