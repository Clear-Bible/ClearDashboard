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


            var bearerTokenEncrypted =
                "Gjg4AdLk5TVU02iIdOPZFgbNqsXxSZJXKxGuGPhoCZuZ/Jgd/nQs8Zlx9ni+TBFXmnJXzyQkdqXBJtEFX6fj3wPCU97E5sCsSX4OYz8Hzhyj8" 
                + "/WQYXjSw3V6nmJ6Rweg6M4e+HCyPj3varHGUGBW+WH2jMCeKUOsiVnhqkfXTHYzx0Min4wmnY2FR4M+1rNkNx4cbyO3yD5RkCQ+ZcviX2kq"
                + "mNgSQfRVLeVfknE/N87jI5C7q/5No3bHyDdOqcFSXo5XTeuP5Vu2hAPorDWlAbd2gDgpftt15wx4MFprzsS1v/XsB8QQnXQ3X+u6sSYeNRy" 
                + "/kFdYOJxY36wYsWwbvC+h7JZV9GerY49zcWVlpKtF6R8YPOogZuFv6g3N2GOYHRhspip+kJALMclAEBan7hqIb9a8u097eR11NJ1B062c" 
                + "/arYfRqob7sdGuM4QR7AYKAeg0i1EjMtFNA3a8FUmJwi4+mpUS8Ak2x5I662jW3CJknbuOFV4piA6gwMmiP0DBf0c8Z2bL+/Dpvpf4clLkl" 
                + "0DM066fmGnzWv7NlzQwTO2JNb2Ed0T9Hu+Ong91Z81Q84NXFGw2BTOlpFUagNjwFCmk+6XUmA+NG5J41bO87m+pfuCvmETCY31v4HI/a7As" 
                + "VUcmbGC9e9MkaWxYmyMBmLZBCaD4CzlUFQzmn0JzCxWuTdeq/mDVQlXOXcrepOZgDr6D2OX6NYgZJvTko3Znj6bjx7mr3wdTnKs89O37Gt6" 
                + "HIcu+FeOIUz7rhDqnqR";
            value = Encryption.Decrypt(bearerTokenEncrypted);
            // add in a service for the MySQL API
            serviceCollection.AddHttpClient<MySqlClient>("MySqlClient", client =>
            {
                // Other settings
                client.BaseAddress = new Uri(Settings.Default.MySqlRootUrl); //"https://mysqluser.cleardashboard.org"
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
