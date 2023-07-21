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
using System.Net.Http;


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
            serviceCollection.AddSingleton<GitLabHttpClientServices>();

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



            serviceCollection.AddSingleton<CollaborationServerHttpClientServices>();

            var bearerTokenEncrypted = Settings.Default.BearerTokenEncrypted;
            value = Encryption.Decrypt(bearerTokenEncrypted);
            // add in a service for the MySQL Collaboration API
            serviceCollection.AddHttpClient<CollaborationClient>("CollaborationClient", client =>

            {
                // Other settings
                client.BaseAddress = new Uri(Settings.Default.CollaborationRootUrl); //"https://collaborationapi.cleardashboard.org"
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

        public static CollaborationServerHttpClientServices GetSqlHttpClientServices()
        {
            var bearerTokenEncrypted = Settings.Default.BearerTokenEncrypted;
            var value = Encryption.Decrypt(bearerTokenEncrypted);

            var client = new HttpClient();

            client.BaseAddress = new Uri(Settings.Default.CollaborationRootUrl); //"https://mysqlapi.cleardashboard.org"
            client.DefaultRequestHeaders.Add("Accept", "*/*");
            client.DefaultRequestHeaders.Add("User-Agent", "HttpClientFactory-ClearDashboard");
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + value);

            var mySqlClient = new CollaborationClient(client);
            var mySqlHttpClientServices = new CollaborationServerHttpClientServices(mySqlClient);

            return mySqlHttpClientServices;
        }


        public static GitLabHttpClientServices GetGitLabHttpClientServices()
        {
            var value = Encryption.Decrypt("IhxlhV+rjvducjKx0q2TlRD4opTViPRm5w/h7CvsGcLXmSAgrZLX1pWFLLYpWqS3");
            var client = new HttpClient();

            client.BaseAddress = new Uri(Settings.Default.GitRootUrl); //"https://gitlab.cleardashboard.org/api/v4/"
            client.DefaultRequestHeaders.Add("Accept", "*/*");
            client.DefaultRequestHeaders.Add("User-Agent", "HttpClientFactory-ClearDashboard");
            client.DefaultRequestHeaders.Add("Authorization", value);

            var gitLabClient = new GitLabClient(client);
            var getGitLabHttpClientServices = new GitLabHttpClientServices(gitLabClient);

            return getGitLabHttpClientServices;
        }
    }
} 
