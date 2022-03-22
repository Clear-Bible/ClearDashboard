using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClearDashboard.DataAccessLayer.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ClearDashboard.DataAccessLayer.Extensions
{
    public static class ServiceCollectionExtensions
    {
        //public static void AddAlignmentDatabase(this IServiceCollection serviceCollection, string connectionString)
        //{
        //    serviceCollection.AddScoped<AlignmentContext>(sp => AlignmentContext.Create(connectionString));
        //}

        public static void AddProjectNameDatabaseContextFactory(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<AlignmentContext>();
            serviceCollection.AddScoped<ProjectNameDbContextFactory>();
        }
    }
}
