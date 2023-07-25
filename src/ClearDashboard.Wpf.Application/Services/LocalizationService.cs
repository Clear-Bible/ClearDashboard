using System;
using System.Threading;
using ClearApplicationFoundation.Services;
using ClearDashboard.Wpf.Application.Strings;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.Wpf.Application.Services;

public class LocalizationService : ILocalizationService
{
    private readonly ILogger<LocalizationService> _logger;

    public LocalizationService(ILogger<LocalizationService> logger)
    {
        _logger = logger;
    }


    public string this[string key] => Get(key);

    public string Get(string key)
    {
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