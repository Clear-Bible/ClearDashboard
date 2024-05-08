using Caliburn.Micro;
using ClearApplicationFoundation.Framework;
using ClearDashboard.Wpf.Application.Helpers;
using ClearDashboard.Wpf.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace ClearDashboard.Wpf.Application.Extensions
{
    public static class ServiceCollectionExtensionsWpf
    {

        public static void AddLocalization(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<TranslationSource>();
            serviceCollection.AddTransient<ILocalizationService, LocalizationService>();
        }

        public static FrameSet AddCaliburnMicro(this IServiceCollection serviceCollection)
        {
            var frameSet = new FrameSet();
            // wire up the interfaces required by Caliburn.Micro
            serviceCollection.AddSingleton<IWindowManager, WindowManager>();
            serviceCollection.AddSingleton<IEventAggregator, EventAggregator>();

            // Register the FrameAdapter which wraps a Frame as INavigationService
            serviceCollection.AddSingleton<INavigationService>(sp => frameSet.NavigationService);

            // wire up all of the view models in the project.
            typeof(Bootstrapper).Assembly.GetTypes()
                .Where(type => type.IsClass)
                .Where(type => type.IsAbstract == false) // ignore any view models which are abstract!
                .Where(type => type.Name != "ShellViewModel" && type.Name != "MainViewModel" && type.Name.EndsWith("ViewModel"))
                .ToList()
                .ForEach(viewModelType => serviceCollection.AddTransient(viewModelType));

            typeof(Bootstrapper).Assembly.GetTypes()
                .Where(type => type.IsClass)
                .Where(type => type.IsAbstract == false) // ignore any view which are abstract!
                .Where(type => type.Name != "ShellView" && type.Name != "MainView" && type.Name.EndsWith("View"))
                .ToList()
                .ForEach(viewType => serviceCollection.AddTransient(viewType));

            return frameSet;
        }
    }
}
