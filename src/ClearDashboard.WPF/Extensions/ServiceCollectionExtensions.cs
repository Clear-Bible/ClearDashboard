using System.Linq;
using Caliburn.Micro;
using Microsoft.Extensions.DependencyInjection;
namespace ClearDashboard.Wpf.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddCaliburnMicro(this IServiceCollection serviceCollection, FrameSet frameSet)
        {
            // wire up the interfaces required by Caliburn.Micro
            serviceCollection.AddSingleton<IWindowManager, WindowManager>();
            serviceCollection.AddSingleton<IEventAggregator, EventAggregator>();

            // Register the FrameAdapter which wraps a Frame as INavigationService
            serviceCollection.AddSingleton<INavigationService>(sp => frameSet.NavigationService);

            // wire up all of the view models in the project.
            typeof(Bootstrapper).Assembly.GetTypes()
                .Where(type => type.IsClass)
                .Where(type => type.Name.EndsWith("ViewModel"))
                .ToList()
                .ForEach(viewModelType => serviceCollection.AddScoped(viewModelType));
        }
    }
}
