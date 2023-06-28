using ClearDashboard.DAL.Alignment.Features.Corpora;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer;
using ClearDashboard.DataAccessLayer.BackgroundServices;
using ClearDashboard.DataAccessLayer.Features;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Paratext;
using ClearDashboard.Wpf.Application.Services;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System;



namespace ClearDashboard.Wpf.Application.Extensions
{

    public static class ServiceCollectionExtensions
    {
        public static void AddClearDashboardDataAccessLayer(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddLogging();

            serviceCollection.AddMediatR(typeof(IMediatorRegistrationMarker), typeof(CreateTokenizedCorpusFromTextCorpusCommandHandler));
            
            serviceCollection.AddSingleton<DashboardProjectManager>();
            serviceCollection.AddSingleton<ProjectManager, DashboardProjectManager>(sp => sp.GetService<DashboardProjectManager>() ?? throw new InvalidOperationException());
            serviceCollection.AddSingleton<IUserProvider, DashboardProjectManager>(sp => sp.GetService<DashboardProjectManager>() ?? throw new InvalidOperationException());
            serviceCollection.AddSingleton<IProjectProvider, DashboardProjectManager>(sp => sp.GetService<DashboardProjectManager>() ?? throw new InvalidOperationException());
            serviceCollection.AddSingleton<GitLabClient>();
            serviceCollection.AddSingleton<HttpClientServices>();

            var value = Encryption.Decrypt("IhxlhV+rjvducjKx0q2TlRD4opTViPRm5w/h7CvsGcLXmSAgrZLX1pWFLLYpWqS3");
            // add in a service for the GitLab repository
            serviceCollection.AddHttpClient<GitLabClient>("GitLabClient", client =>
            {
                // Other settings
                client.BaseAddress = new Uri(Settings.Default.GitRootUrl); //"https://gitlab.cleardashboard.org/api/v4/"
                client.DefaultRequestHeaders.Add("Accept", "*/*");
                client.DefaultRequestHeaders.Add("User-Agent", "HttpClientFactory-ClearDashboard");
                client.DefaultRequestHeaders.Add("Authorization", value);
            });



            serviceCollection.AddSingleton<MySqlHttpClientServices>();

            var bearerTokenEncrypted = Settings.Default.BearerTokenEncrypted;
            value = Encryption.Decrypt(bearerTokenEncrypted);
            // add in a service for the MySQL API
            serviceCollection.AddHttpClient<MySqlClient>("MySqlClient", client =>
            {
                // Other settings
                client.BaseAddress = new Uri(Settings.Default.MySqlRootUrl); //"https://mysqlapi.cleardashboard.org"
                client.DefaultRequestHeaders.Add("Accept", "*/*");
                client.DefaultRequestHeaders.Add("User-Agent", "HttpClientFactory-ClearDashboard");
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + value);
            });


            serviceCollection.AddScoped<ParatextProxy>();
            
            // QUESTION:  Can we run the HostedService as a scoped service?
            // ANSWER:    Testing seems to indicate, YES!

            //serviceCollection.AddHostedService<ClearEngineBackgroundService>();
            serviceCollection.AddScoped<ClearEngineBackgroundService>();
            serviceCollection.AddScoped<IClearEngineProcessingService, ClearEngineProcessingService>();
        }
    }
} 
