using System;
using ClearDashboard.DataAccessLayer.Features;
using ClearDashboard.DataAccessLayer.Wpf.Extensions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace ClearDashboard.DataAccessLayer.Wpf
{
    public abstract class RecipeManager
    {
        protected readonly ServiceCollection Services = new ServiceCollection();
        private IServiceProvider? _serviceProvider = null;
        protected IServiceProvider ServiceProvider => _serviceProvider ??= Services.BuildServiceProvider();

        
        #pragma warning disable CS8603 // Possible null reference return.

        // Get an instance of Mediator honoring the lifetime configured when Mediator
        // is added to the ServiceCollection.  The default is Transient
        protected IMediator Mediator => ServiceProvider.GetService<IMediator>();

        

        #pragma warning restore CS8603 // Possible null reference return.

        protected RecipeManager()
        {
            // ReSharper disable once VirtualMemberCallInConstructor
            SetupDependencyInjection();
        }


        protected virtual void SetupDependencyInjection()
        {
            Services.AddClearDashboardDataAccessLayer();
            Services.AddMediatR(typeof(IMediatorRegistrationMarker));
            Services.AddLogging();
        }
    }
    
}
