using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ClearDashboard.DataAccessLayer.Context
{
    public interface IProjectNameDbContextFactory<out TDbContext> where TDbContext : DbContext
    {
        TDbContext Create(string connectionString);
    }
    public class ProjectNameDbContextFactory : IProjectNameDbContextFactory<AlignmentContext>
    {
        private readonly IServiceProvider _serviceProvider;

        public ProjectNameDbContextFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        public AlignmentContext Create(string projectName)
        {
            var directoryPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.Personal)}\\{projectName}";
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            var fullPath = $"{directoryPath}\\{projectName}.sqlite";
           return AlignmentContext.Create(fullPath);
        }
    }
}
