using System;
using System.Globalization;
using System.Resources;
using System.Threading;
using ClearDashboard.Wpf.Application.Services;
using ClearDashboard.Wpf.Application.Strings;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.Wpf.Application.Helpers
{

    public class LocalizationService : ILocalizationService
    {
        private readonly ILogger<LocalizationService> _logger;

        public LocalizationService(ILogger<LocalizationService> logger)
        {
            _logger = logger;
        }
        public string Get(string key)
        {

            //var resourceManager = new ResourceManager(typeof("ClearDashboard.Aqua.Module.Strings.Resources"))
            //var resourceSet = resourceManager.GetResourceSet(CultureInfo.GetCultureInfo("en-us"), true, true);
            //if (resourceSet == null)
            //    throw new Exception("Unable to create ResourceSet.");

            //var s = resourceSet.GetString("TestKey");


            string localizedString;
            try
            {
                localizedString = Resources.ResourceManager.GetString(key, Thread.CurrentThread.CurrentUICulture);
            }
            catch (Exception e)
            {
                _logger.LogCritical($"Localization string missing for key '{key}' {e.Message} {Thread.CurrentThread.CurrentUICulture.Name}");
                localizedString = key;
            }

            if (localizedString == null)
            {
                _logger.LogCritical($"Localization string missing for key '{key}' {Thread.CurrentThread.CurrentUICulture.Name}");
                localizedString = key;
            }
            return localizedString;
        }
    }

    [Obsolete("This class has been deprecated and will be removed in a future release.  Please inject ILocalizationService instead.")]
    public static class LocalizationStrings
    {
        public static string? Get(string? key, ILogger logger)
        {
            string? localizedString;
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
