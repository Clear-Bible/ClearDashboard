using ClearDashboard.Wpf.Application.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Reflection;
using System.Resources;
using System.Threading;

namespace ClearDashboard.Paranext.Module.Services
{
    public class ParanextLocalizationService : ILocalizationService
    {

        private readonly ILogger<ParanextLocalizationService> _logger;

        public ParanextLocalizationService(ILogger<ParanextLocalizationService> logger)
        {
            _logger = logger;
        }

        public string this[string key] => Get(key);

        public string Get(string key)
        {
            string localizedString;
            try
            {
                var resourceManager = new ResourceManager("ClearDashboard.Paranext.Module.Strings.Resources", Assembly.GetExecutingAssembly());
                var resourceSet = resourceManager.GetResourceSet(Thread.CurrentThread.CurrentUICulture, true, true);
                if (resourceSet == null)
                {
                    _logger.LogCritical(
                        $"Localization string missing for key '{key}' {Thread.CurrentThread.CurrentUICulture.Name}");
                    return key;
                }
                localizedString = resourceSet.GetString(key)!;
            }
            catch (Exception e)
            {
                _logger.LogCritical(
                    $"Localization string missing for key '{key}' {e.Message} {Thread.CurrentThread.CurrentUICulture.Name}");
                localizedString = key;
            }

            if (string.IsNullOrEmpty(localizedString))
            {
                _logger.LogCritical(
                    $"Localization string missing for key '{key}' {Thread.CurrentThread.CurrentUICulture.Name}");
                localizedString = key;
            }

            return localizedString;
        }
    }
}
