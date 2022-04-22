using Owin;
using System.Net.Http.Headers;
using System.Web.Http;
using Serilog;
using System;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Web.Http.Dependencies;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Web.Http.Controllers;
using ClearDashboard.WebApiParatextPlugin.Controllers;
using Paratext.PluginInterfaces;

namespace ClearDashboard.WebApiParatextPlugin
{
    public class Startup
    {

        private static IProject _project;
        private int _bookNumber;
        private static IVerseRef _verseRef;
        private IReadOnlyList<IProjectNote> _noteList;
        private IWindowPluginHost _host;
        private IPluginChildWindow _parent;

        public Startup(IProject project, IVerseRef verseRef)
        {
            _project = project;
            _verseRef = verseRef;
        }

        // This code configures Web API. The Startup class is specified as a type
        // parameter in the WebApp.Start method.
        public void Configuration(IAppBuilder appBuilder)
        {
            try
            {
                // Configure Web API for self-host. 
                HttpConfiguration config = new HttpConfiguration();

                var serviceProvider = SetupDependencyInjection();
                config.DependencyResolver = new DefaultDependencyResolver(serviceProvider);

              
                config.Formatters.Remove(config.Formatters.XmlFormatter);
                config.Formatters.JsonFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/json"));
                config.Routes.MapHttpRoute(
                    name: "DefaultApi",
                    routeTemplate: "api/{controller}/{id}",
                    defaults: new { id = RouteParameter.Optional }
                );

                appBuilder.UseWebApi(config);
            }
            catch(Exception ex)
            {
                Log.Error(ex, "An unexpected error occurred while configuring Web API.");
            }
           
        }

        private static IServiceProvider SetupDependencyInjection()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            //services.AddSerilog();

            services.AddSingleton<IProject>(sp => _project);
            services.AddSingleton<IVerseRef>(sp => _verseRef);
            services.AddScoped<HelloMessageFactory>();

            services.AddControllersAsServices(typeof(Startup).Assembly.GetExportedTypes()
                .Where(t => !t.IsAbstract && !t.IsGenericTypeDefinition)
                .Where(t => typeof(IHttpController).IsAssignableFrom(t)
                            || t.Name.EndsWith("Controller", StringComparison.OrdinalIgnoreCase)));

            var serviceProvider = services.BuildServiceProvider();

            return serviceProvider;
        }


    }

    public class DefaultDependencyResolver : IDependencyResolver
    {
        private IServiceScope serviceScope;
        protected IServiceProvider ServiceProvider { get; set; }

        public DefaultDependencyResolver(IServiceProvider serviceProvider)
        {
            this.ServiceProvider = serviceProvider;
        }

        public object GetService(Type serviceType)
        {
            return this.ServiceProvider.GetService(serviceType);
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            return this.ServiceProvider.GetServices(serviceType);
        }

        public IDependencyScope BeginScope()
        {
            serviceScope = this.ServiceProvider.CreateScope();
            return new DefaultDependencyResolver(serviceScope.ServiceProvider);
        }

        public void Dispose()
        {
            // you can implement this interface just when you use .net core 2.0
            // this.ServiceProvider.Dispose();

            //need to dispose the scope otherwise
            //you'll get a memory leak
            serviceScope?.Dispose();
        }
    }

    public static class ServiceProviderExtensions
    {
        public static IServiceCollection AddControllersAsServices(this IServiceCollection services,
            IEnumerable<Type> controllerTypes)
        {
            foreach (var type in controllerTypes)
            {
                services.AddScoped(type);
            }

            return services;
        }
    }
}
