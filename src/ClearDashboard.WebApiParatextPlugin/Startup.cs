using Owin;
using System.Net.Http.Headers;
using System.Web.Http;
using Serilog;
using System;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Controllers;
using ClearDashboard.WebApiParatextPlugin.Mvc;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin.Cors;
using Paratext.PluginInterfaces;

namespace ClearDashboard.WebApiParatextPlugin
{
    public class WebHostStartup
    {
        // The following are used to inject Singleton instances
        private static IProject _project;
        private static IVerseRef _verseRef;
        private static MainWindow _mainWindow;

        public IServiceProvider ServiceProvider { get; private set; }

        //public Microsoft.AspNet.SignalR.DefaultDependencyResolver SignalRServiceResolver { get; private set; }

        public WebHostStartup(IProject project, IVerseRef verseRef, MainWindow mainWindow)
        {
            _project = project;
            _verseRef = verseRef;
            _mainWindow = mainWindow;
        }

        // This code configures Web API. The Startup class is specified as a type
        // parameter in the WebApp.Start method.
        public void Configuration(IAppBuilder appBuilder)
        {
            try
            {
                ServiceProvider = SetupDependencyInjection();
                appBuilder.UseCors(CorsOptions.AllowAll);
                appBuilder.Map("/cors", map =>
                {
                    map.UseCors(CorsOptions.AllowAll);
                    map.MapSignalR(new HubConfiguration()
                    {
#if DEBUG
                        EnableDetailedErrors = true,
#endif
                    });
                });

                // SignalRServiceResolver = new Microsoft.AspNet.SignalR.DefaultDependencyResolver();
                appBuilder.MapSignalR(new HubConfiguration()
                {
#if DEBUG
                    EnableDetailedErrors = true,
                    //  Resolver = SignalRServiceResolver
#endif
                });

                // GlobalHost.DependencyResolver = SignalRServiceResolver;

                var config = InitializeHttpConfiguration();
                appBuilder.UseWebApi(config);

            }
            catch(Exception ex)
            {
                Log.Error(ex, "An unexpected error occurred while configuring Web API.");
            }
           
        }

        private HttpConfiguration InitializeHttpConfiguration()
        {
            var config = new HttpConfiguration();
            config.DependencyResolver = new DefaultDependencyResolver(ServiceProvider);
            config.MessageHandlers.Add(new MessageLoggingHandler(_mainWindow));
            config.Formatters.Remove(config.Formatters.XmlFormatter);
            config.Formatters.JsonFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/json"));
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            config.EnsureInitialized(); //Nice to check for issues before first request

            return config;
        }

        private IServiceProvider SetupDependencyInjection()
        {
            var services = new ServiceCollection();
            services.AddLogging();

            services.AddSingleton<MainWindow>(sp => _mainWindow);
            //services.AddSerilog();
            
            services.AddSingleton<IProject>(sp => _project);
            services.AddSingleton<IVerseRef>(sp => _verseRef);
           
            services.AddControllersAsServices(typeof(WebHostStartup).Assembly.GetExportedTypes()
                .Where(t => !t.IsAbstract && !t.IsGenericTypeDefinition)
                .Where(t => typeof(IHttpController).IsAssignableFrom(t)
                            || t.Name.EndsWith("Controller", StringComparison.OrdinalIgnoreCase)));

            var serviceProvider = services.BuildServiceProvider();
            return serviceProvider;
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
