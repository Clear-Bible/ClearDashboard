using Caliburn.Micro;
using Microsoft.Extensions.Logging;
using System;

namespace ClearDashboard.Wpf.ViewModels
{
    public class ApplicationScreen : Screen, IDisposable
    {
        protected ILogger Logger { get; private set; }
        protected INavigationService NavigationService { get; private set; }

        public ApplicationScreen()
        {

        }

        public ApplicationScreen(INavigationService navigationService, ILogger logger)
        {
            NavigationService = navigationService;
            Logger = logger;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // dispose of unmanaged resources here...
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
