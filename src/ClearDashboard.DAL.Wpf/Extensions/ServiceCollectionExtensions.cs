using ClearDashboard.DAL.Alignment.Features.Corpora;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.BackgroundServices;
using ClearDashboard.DataAccessLayer.Data;
using ClearDashboard.DataAccessLayer.Data.Interceptors;
using ClearDashboard.DataAccessLayer.Features;
using ClearDashboard.DataAccessLayer.Paratext;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace ClearDashboard.DataAccessLayer.Wpf.Extensions
{
    public static class ServiceCollectionExtensions
    {
        private static void AddProjectNameDatabaseContextFactory(this IServiceCollection serviceCollection)
        {
          
            serviceCollection.AddScoped<ProjectDbContext>();
            serviceCollection.AddScoped<ProjectDbContextFactory>();
            serviceCollection.AddScoped<SqliteDatabaseConnectionInterceptor>();
        }

        public static void AddClearDashboardDataAccessLayer(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddLogging();

            serviceCollection.AddMediatR(typeof(IMediatorRegistrationMarker), typeof(CreateTokenizedCorpusFromTextCorpusCommandHandler));
            
            serviceCollection.AddSingleton<DashboardProjectManager>();
            serviceCollection.AddSingleton<ProjectManager, DashboardProjectManager>(sp => sp.GetService<DashboardProjectManager>() ?? throw new InvalidOperationException());
            serviceCollection.AddTransient<IUserProvider, DashboardProjectManager>(sp => sp.GetService<DashboardProjectManager>() ?? throw new InvalidOperationException());
            serviceCollection.AddTransient<IProjectProvider, DashboardProjectManager>(sp => sp.GetService<DashboardProjectManager>() ?? throw new InvalidOperationException());


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
