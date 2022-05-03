using System;
using System.Collections.Generic;
using System.Web.Http.Dependencies;
using Microsoft.Extensions.DependencyInjection;

namespace ClearDashboard.WebApiParatextPlugin
{
    public class DefaultDependencyResolver : IDependencyResolver
    {
        private IServiceScope _serviceScope;
        protected IServiceProvider ServiceProvider { get; set; }

        public DefaultDependencyResolver(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }

        public object GetService(Type serviceType)
        {
            return ServiceProvider.GetService(serviceType);
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            return ServiceProvider.GetServices(serviceType);
        }

        public IDependencyScope BeginScope()
        {
            _serviceScope = ServiceProvider.CreateScope();
            return new DefaultDependencyResolver(_serviceScope.ServiceProvider);
        }

        public void Dispose()
        {
            // you can implement this interface just when you use .net core 2.0
            // this.ServiceProvider.Dispose();
            
            //need to dispose the scope otherwise
            //you'll get a memory leak
            _serviceScope?.Dispose();
        }
    }
}