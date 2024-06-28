using ClearDashboard.Wpf.Application.Services;

namespace ClearDashboard.Wpf.Application.Localization
{
    public static class StaticLocalizationService
    {
        public static ILocalizationService? Localization { get; private set; }

        public static void SetLocalizationService(ILocalizationService localization)
        {
            Localization = localization;
        }
    }
}
