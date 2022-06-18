using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DAL.Tests.Models;
using ClearDashboard.DataAccessLayer.Data;
using ClearDashboard.DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace ClearDashboard.DAL.Tests.Data
{
    public  class TestContext : DbContext
    {

        private readonly ILogger<TestContext>? _logger;
        public string DatabasePath { get; set; }

        public TestContext() : this(string.Empty)
        {

        }

        public TestContext(ILogger <TestContext> logger) : this(string.Empty)
        {
            _logger = logger;
           
        }

        public TestContext(DbContextOptions<AlignmentContext> options)
            : base(options)
        {
            DatabasePath = string.Empty;
        }

        protected TestContext(string databasePath)
        {
            DatabasePath = databasePath;
        }

        public virtual DbSet<GrandParent> GrandParents => Set<GrandParent>();
        public virtual DbSet<Parent> Parents => Set<Parent>();
        public virtual DbSet<Child> Children => Set<Child>();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite($"Filename={DatabasePath}");
            }
        }

        public async Task Migrate()
        {
            try
            {
                // Ensure that the database is created.  Note that if we want to be able to apply migrations later,
                // we want to call Database.Migrate(), not Database.EnsureCreated().
                // https://stackoverflow.com/questions/38238043/how-and-where-to-call-database-ensurecreated-and-database-migrate
                _logger?.LogInformation("Ensuring that the database is created, migrating if necessary.");

                await Database.MigrateAsync();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Could not apply database migrations");

                // This is useful when applying migrations via the EF core plumbing, please leave in place.
                Console.WriteLine($"Could not apply database migrations: {ex.Message}");
            }
        }

        public EntityEntry<TEntity> AddCopy<TEntity>(TEntity entity) where TEntity : class, new()
        {
            Entry(entity).State = EntityState.Detached;
            var newEntity = CreateEntityCopy(entity);
            return Add(newEntity);
        }

        public async Task<EntityEntry<TEntity>> AddCopyAsync<TEntity>(TEntity entity) where TEntity : class, new()
        {
            Entry(entity).State = EntityState.Detached;

            var e = (TEntity)Entry(entity).CurrentValues.ToObject();
            var newEntity = CreateEntityCopy(entity);
            return await AddAsync(newEntity);
        }

        private TEntity CreateCopy<TEntity>(TEntity entity)
        {
            var json = JsonConvert.SerializeObject(entity);
            return JsonConvert.DeserializeObject<TEntity>(json);
        }

        private static TEntity CreateEntityCopy<TEntity>(TEntity entity) where TEntity : class, new()
        {
            var newEntity = new TEntity();
            var propertyNamesToIgnore = new List<string> { "Id", "ParentId", "Created", "Modified" };
            var currentIdProperty = entity.GetType().GetProperty("Id");
            if (currentIdProperty != null)
            {
                var properties = entity.GetType().GetProperties()
                    .Where(property => !propertyNamesToIgnore.Contains(property.Name));
                foreach (var propertyInfo in properties)
                {
                    if (propertyInfo.PropertyType == typeof(ICollection<>))
                    {

                        var collectionObject = propertyInfo.GetValue(entity, null);
                        var collection = Convert.ChangeType(collectionObject, propertyInfo.PropertyType);
                        //foreach (var o in collection)
                        //{
                        //    //var e = CreateEntityCopy<TEntity>(o);
                        //}

                    }
                    else
                    {
                        var property = propertyInfo.GetValue(entity, null);
                        propertyInfo.SetValue(newEntity, property);
                    }

                }

                var parentId = currentIdProperty.GetValue(entity, null);
                var parentIdProperty = entity.GetType().GetProperty("ParentId");
                parentIdProperty.SetValue(newEntity, parentId);
            }

            return newEntity;
        }
    }
}
