using System;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace ClearDashboard.DataAccessLayer.Wpf
{
    public abstract class ClearEngineRecipeFactoryBase
    {

        protected IServiceProvider? ServiceProvider { get; private set; }
        protected IServiceCollection? ServiceCollection { get; private set; }

        protected ClearEngineRecipeFactoryBase()
        {
            // ReSharper disable VirtualMemberCallInConstructor
            InternalSetupDependencyInjection();
            // ReSharper restore VirtualMemberCallInConstructor
        }

        protected abstract IServiceProvider? SetupDependencyInjection();

        private void InternalSetupDependencyInjection()
        {
            ServiceCollection = new ServiceCollection();

            ServiceCollection.AddMediatR(typeof(ClearEngineRecipeFactoryBase));

            ServiceProvider = SetupDependencyInjection();
        }
        
    }
}
