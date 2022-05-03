using System.Threading.Tasks;
using ClearDashboard.DataAccessLayer.BackgroundServices;
using ClearDashboard.DataAccessLayer.Data;
using ClearDashboard.DataAccessLayer.Features.ManuscriptVerses;
using ClearDashboard.DataAccessLayer.Paratext;
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
            
            serviceCollection.AddSingleton<DashboardProjectManager>();
            serviceCollection.AddScoped<ParatextProxy>();
            serviceCollection.AddProjectNameDatabaseContextFactory();

            // QUESTION:  Can we run the HostedService as a scoped service?
            // ANSWER:    Testing seems to indicate, YES!

            //serviceCollection.AddHostedService<ClearEngineBackgroundService>();
            serviceCollection.AddScoped<ClearEngineBackgroundService>();
            serviceCollection.AddScoped<IClearEngineProcessingService, ClearEngineProcessingService>();
        }
    }
} 
