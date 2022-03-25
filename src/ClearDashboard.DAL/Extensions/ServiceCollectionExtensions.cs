using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClearDashboard.DataAccessLayer.Context;
using ClearDashboard.DataAccessLayer.NamedPipes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.DataAccessLayer.Extensions
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

            // add in the DAL
            serviceCollection.AddSingleton<StartUp>();
            serviceCollection.AddSingleton<NamedPipesClient>(sp =>
            {
                var logger = sp.GetService<ILogger<NamedPipesClient>>();
                var namedPipesClient = new NamedPipesClient();
                namedPipesClient.InitializeAsync().ContinueWith(t =>
                       logger.LogError($"Error while connecting to pipe server: {t.Exception}"),
                    TaskContinuationOptions.OnlyOnFaulted);
                return namedPipesClient;
            });

            serviceCollection.AddProjectNameDatabaseContextFactory();
        }
    }
} 
