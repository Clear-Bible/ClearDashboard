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

            var bearerTokenEncrypted =
                "Gjg4AdLk5TVU02iIdOPZFgbNqsXxSZJXKxGuGPhoCZuZ/Jgd/nQs8Zlx9ni+TBFXZEnwUjB4TQT085Oxnwq7rhdII2mUFS28DhCJUkF36mQI7RBRzJOwSPF42QA1iQCqH9A235Li4nzLdtu/VEfNQ63dW40bA/TWM5IbSL426aBvJeg30iJzFWP4XR/poqSrAaCWZya5F6d7Cvcwx+5/Pq6G+jfdk9bNp6vQwtVdpgStFRY1uS6dLX1hXn73sdZjHsS4+587e5MnROMT0LUWeiDjzvWMJ1Mm/dWBGl040tdOr2VngGpuHqWULb1V4KcS+KqNPqZY4Ugqayr7Noo4KzOrdelFpTci7euVtyG5z2SbxXDmqe8Xgbj2TLMVkjEjt68wKdq6pfsgCaZFJeTwhV2jO7/uKOZOImUlwBpGmqm/18KuLJK/NH03ApsyFU2fplxczyfDO/y3WV3o1G6RqugTKdjLU/GJgDjwhyYYsxaIKDBd0Ie+wm6QKDBZ+OLMa9jvat0AfvqXDI7qkmgq8HGalEMNWPfY+RrV7BvIDnJ0wMP5kUWbtgqwP5CIsS0++cLHOyV1dsFh/QwBoFDYHmO5Qvv4D4DeocUmfPVbjpO2hKKGl7BP+7+ctFRozrWmjVTRXkkLGT5cf+K7CRbdv+gBMZaYTGlsxzKr24GuiMNJlCYkrj9dfXMBomkG8DG4NwJGx+v110W7FaOHkcmc1qlwqMd882i8HRnvE+TPw9wPUXkEmvxnkyksjLqkI+Rd";
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
