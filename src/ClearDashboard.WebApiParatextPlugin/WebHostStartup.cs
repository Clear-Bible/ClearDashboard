using ClearDashboard.ParatextPlugin.CQRS.Features.Lexicon;
using ClearDashboard.WebApiParatextPlugin.Features.Lexicon;
using ClearDashboard.WebApiParatextPlugin.Features.Project;
using MediatR;
using Microsoft.AspNet.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Owin.Cors;
using Owin;
using Paratext.PluginInterfaces;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace ClearDashboard.WebApiParatextPlugin
{
    public class WebHostStartup
    {
        // The following are used to inject Singleton instances
        private static IProject _project;
        private static IVerseRef _verseRef;
        private static MainWindow _mainWindow;
        private static IPluginHost _pluginHost;
        private static IPluginChildWindow _parent;
        private static IPluginLogger _pluginLogger;

        public static IServiceProvider ServiceProvider { get; private set; }


        public WebHostStartup(IProject project, IVerseRef verseRef, MainWindow mainWindow, IPluginHost pluginHost,
            IPluginChildWindow parent, IPluginLogger pluginLogger)
        {
            _project = project;
            _verseRef = verseRef;
            _mainWindow = mainWindow;
            _pluginHost = pluginHost;
            _parent = parent;
            _pluginLogger = pluginLogger;
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

                //var signalRServiceResolver = new CustomSignalRDependencyResolver(ServiceProvider);
                appBuilder.MapSignalR(new HubConfiguration()
                {
#if DEBUG
                    EnableDetailedErrors = true,

                    //Resolver = signalRServiceResolver
#endif
                });

                //GlobalHost.DependencyResolver = signalRServiceResolver;

                var config = InitializeHttpConfiguration();
                appBuilder.UseWebApi(config);

            }
            catch (Exception ex)
            {
                Log.Error(ex, "An unexpected error occurred while configuring Web API.");
            }

        }

        private HttpConfiguration InitializeHttpConfiguration()
        {
            var config = new HttpConfiguration();
            config.DependencyResolver = new DefaultDependencyResolver(ServiceProvider);
            // config.MessageHandlers.Add(new MessageLoggingHandler(_mainWindow));
            config.Formatters.Remove(config.Formatters.XmlFormatter);
            //config.Formatters.JsonFormatter.SerializerSettings = new JsonSerializerSettings
            //    { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };
            config.Formatters.JsonFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/json"));

            config.Routes.MapHttpRoute(
                name: "ControllerAndActionOnly",
                routeTemplate: "api/{controller}/{action}",
                defaults: new { }
            );

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

            services.AddMediatR(typeof(GetCurrentProjectQueryHandler));


            services.AddTransient<IProject>(sp => _project);
            services.AddTransient<IVerseRef>(sp => _verseRef);
            services.AddSingleton<IPluginHost>(sp => _pluginHost);
            services.AddSingleton<IPluginChildWindow>(sp => _parent);
            services.AddSingleton<IPluginLogger>(sp => _pluginLogger);

            Func<string, DataAccessLayer.Models.ParatextProjectMetadata> getParatextProjectMetadata = (string projectId) =>
            {
                return _mainWindow.GetProjectMetadata()
                            .Where(e => e.Id == (!string.IsNullOrEmpty(projectId) ? projectId : _mainWindow.Project.ID))
                            .SingleOrDefault();
            };

            services.AddTransient<ILexiconObtainable>(x => 
                new Features.Lexicon.LexiconFromXmlFiles(
                    x.GetRequiredService<ILogger<LexiconFromXmlFiles>>(),
                    getParatextProjectMetadata,
                    Directory.GetCurrentDirectory() /* paratextAppPath */
                ));

            services.AddControllersAsServices(typeof(WebHostStartup).Assembly.GetExportedTypes()
                .Where(t => !t.IsAbstract && !t.IsGenericTypeDefinition)
                .Where(t => typeof(IHttpController).IsAssignableFrom(t)
                            || t.Name.EndsWith("Controller", StringComparison.OrdinalIgnoreCase)));

            var serviceProvider = services.BuildServiceProvider();
            return serviceProvider;
        }

        public void ChangeVerse(IVerseRef verse)
        {
            _verseRef = verse;
        }


        public void ChangeProject(IProject project)
        {
            _project = project;
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

    public class CustomSignalRDependencyResolver : Microsoft.AspNet.SignalR.DefaultDependencyResolver
    {
        private readonly IServiceProvider _serviceProvider;

        public CustomSignalRDependencyResolver(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public override object GetService(Type serviceType)
        {
            return _serviceProvider.GetService(serviceType);
        }

        public override IEnumerable<object> GetServices(Type serviceType)
        {
            return _serviceProvider.GetServices(serviceType);
        }
    }

}
