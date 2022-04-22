using System.Threading.Tasks;
using ClearDashboard.DataAccessLayer.BackgroundServices;
using ClearDashboard.DataAccessLayer.Context;
using ClearDashboard.DataAccessLayer.Data;
using ClearDashboard.DataAccessLayer.NamedPipes;
using ClearDashboard.DataAccessLayer.Paratext;
using ClearDashboard.DataAccessLayer.Slices.ManuscriptVerses;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.DataAccessLayer.Wpf.Extensions
{
    public static class ServiceCollectionExtensions
    {
        private static void AddProjectNameDatabaseContextFactory(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<AlignmentContext>();
            serviceCollection.AddScoped<ProjectNameDbContextFactory>();
        }

        public static void AddClearDashboardDataAccessLayer(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddLogging();

            serviceCollection.AddMediatR(typeof(GetManuscriptVerseByIdQuery));

            serviceCollection.AddSingleton<ProjectManager>();
            serviceCollection.AddScoped<ParatextProxy>();
            serviceCollection.AddSingleton<NamedPipesClient>(sp =>
            {
                var logger = sp.GetService<ILogger<NamedPipesClient>>();
                var namedPipesClient = new NamedPipesClient(logger);
                namedPipesClient.InitializeAsync().ContinueWith(t =>
                    {
                        logger?.LogError($"Error while connecting to pipe server: {t.Exception}");
                    },
                    TaskContinuationOptions.OnlyOnFaulted);
                return namedPipesClient;
            });

            serviceCollection.AddProjectNameDatabaseContextFactory();

            // QUESTION:  Can we run the HostedService as a scoped service?
            // ANSWER:    Testing seems to indicate, YES!

            //serviceCollection.AddHostedService<ClearEngineBackgroundService>();
            serviceCollection.AddScoped<ClearEngineBackgroundService>();
            serviceCollection.AddScoped<IClearEngineProcessingService, ClearEngineProcessingService>();
        }
    }
} 
