using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data.Extensions;
using ClearDashboard.DataAccessLayer.Data.Interceptors;
using ClearDashboard.DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace ClearDashboard.DataAccessLayer.Data
{
    public class ProjectDbContext : DbContext
    {
        private readonly ILogger<ProjectDbContext>? _logger;
        public  IUserProvider? UserProvider { get; set; }
        public string DatabasePath { get; set; }
        private readonly SqliteDatabaseConnectionInterceptor _sqliteDatabaseConnectionInterceptor;
        
        
        public ProjectDbContext() : this(string.Empty)
        {
           
        }

        public ProjectDbContext(ILogger<ProjectDbContext> logger, IUserProvider userProvider, SqliteDatabaseConnectionInterceptor sqliteDatabaseConnectionInterceptor) : this(string.Empty)
        {
            _logger = logger;
            UserProvider = userProvider;
            _sqliteDatabaseConnectionInterceptor = sqliteDatabaseConnectionInterceptor;
        }

        public ProjectDbContext(DbContextOptions<ProjectDbContext> options)
            : base(options)
        {
            DatabasePath = string.Empty;
        }

        protected ProjectDbContext(string databasePath)
        {
            DatabasePath = databasePath;
        }

        public virtual DbSet<Adornment> Adornments => Set<Adornment>();
        public virtual DbSet<Alignment> Alignments => Set<Alignment>();

        public virtual DbSet<AlignmentSet> AlignmentSets => Set<AlignmentSet>();
        public virtual DbSet<AlignmentTokenPair> AlignmentTokenPairs => Set<AlignmentTokenPair>();

        public virtual DbSet<AlignmentVersion> AlignmentVersions => Set<AlignmentVersion>();
        public virtual DbSet<Corpus> Corpa => Set<Corpus>();
        public virtual DbSet<CorpusHistory> CorpaHistory => Set<CorpusHistory>();
        public virtual DbSet<NoteAssociation> NoteAssociations => Set<NoteAssociation>();
        public virtual DbSet<Note> Notes => Set<Note>();
        public virtual DbSet<ParallelCorpus> ParallelCorpa => Set<ParallelCorpus>();
        public virtual DbSet<ParallelCorpusHistory> ParallelCorpaHistory => Set<ParallelCorpusHistory>();
        public virtual DbSet<Project> Projects => Set<Project>();
        //public virtual DbSet<QuestionGroup> QuestionGroups => Set<QuestionGroup>();
        public virtual DbSet<RawContent> RawContent => Set<RawContent>();
        public virtual DbSet<Token> Tokens => Set<Token>();
        public virtual DbSet<TokenizedCorpus> TokenizedCorpora => Set<TokenizedCorpus>();
        public virtual DbSet<User> Users => Set<User>();
        public virtual DbSet<Verse> Verses => Set<Verse>();
        public virtual DbSet<VerseMapping> VerseMappings => Set<VerseMapping>();
        //public virtual DbSet<VerseMappingVerseAssociation> VerseMappingVerseAssociations => Set<VerseMappingVerseAssociation>();
                             
        //public virtual DbSet<ParallelTokenizedCorpus> ParallelTokenizedCorpa => Set<ParallelTokenizedCorpus>();
 

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite($"Filename={DatabasePath}");
            }

            optionsBuilder.AddInterceptors(_sqliteDatabaseConnectionInterceptor);
        }

        public async Task Migrate()
        {
            try
            {
                // Ensure that the database is created.  Note that if we want to be able to apply migrations later,
                // we want to call Database.Migrate(), not Database.EnsureCreated().
                // https://stackoverflow.com/questions/38238043/how-and-where-to-call-database-ensurecreated-and-database-migrate
                //_logger?.LogInformation("Ensuring that the database is created, migrating if necessary.");

                await Database.MigrateAsync();

            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Could not apply database migrations");

                // This is useful when applying migrations via the EF core plumbing, please leave in place.
                Console.WriteLine($"Could not apply database migrations: {ex.Message}");
            }
        }

    

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // We want our table names to be singular
            modelBuilder.RemovePluralizingTableNameConvention();

            // NB:  I'm relying on the default naming and relationship conventions from EF Core to set up the database...

            // **** leaving this here in the event we need to override the default conventions ****
            // NB:  Add the configuration of any newly added 
            //      entities to the ConfigureEntities extension method
            //modelBuilder.ConfigureEntities();

            modelBuilder.Entity<AlignmentTokenPair>()
                .HasOne(e => e.SourceToken)
                .WithMany(e=>e.SourceAlignmentTokenPairs);
              

            modelBuilder.Entity<AlignmentTokenPair>()
                .HasOne(e => e.TargetToken)
                .WithMany(e=>e.TargetAlignmentTokenPairs);

            modelBuilder.Entity<Corpus>()
                .Property(e=>e.Metadata)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, default(JsonSerializerOptions)),
                    v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, default(JsonSerializerOptions)) ?? new Dictionary<string, object>(),
                    new ValueComparer<Dictionary<string, object>>(
                        (c1, c2) => c1.SequenceEqual(c2!),
                        c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                        c => c));

            modelBuilder.Entity<CorpusHistory>()
                .Property(e => e.Metadata)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, default(JsonSerializerOptions)),
                    v => JsonSerializer.Deserialize<Dictionary<string,object>>(v, default(JsonSerializerOptions)) ?? new Dictionary<string, object>(),
                    new ValueComparer<Dictionary<string, object>>(
                        (c1, c2) => c1.SequenceEqual(c2!),
                        c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                        c => c));
              

            modelBuilder.Entity<TokenizedCorpus>()
                .Property(e => e.Metadata)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, default(JsonSerializerOptions)),
                    v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, default(JsonSerializerOptions)) ?? new Dictionary<string, object>(),
                    new ValueComparer<Dictionary<string, object>>(
                        (c1, c2) => c1.SequenceEqual(c2!),
                        c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                        c => c));

            modelBuilder.Entity<ParallelCorpus>()
                .HasOne(e => e.SourceTokenizedCorpus)
                .WithMany(e => e.SourceParallelCorpora);


            modelBuilder.Entity<ParallelCorpus>()
                .HasOne(e => e.TargetTokenizedCorpus)
                .WithMany(e => e.TargetParallelCorpora);

            // NB:  Add any new entities which inherit from RawContent
            //      to the ConfigureRawContentEntities extension method
            modelBuilder.ConfigureRawContentEntities();

            // NB:  Add any new entities which inherit from NoteAssociation
            //      to the ConfigureNoteAssociationEntities extension method
            modelBuilder.ConfigureNoteAssociationEntities();

            // now handle any entities which have DatetimeOffset properties.                                                  
            modelBuilder.AddDateTimeOffsetToBinaryConverter(Database.ProviderName);

            // This ensures that the User Id for the current Dashboard user is always
            // set when an entity is added to the database.
            modelBuilder.AddUserIdValueGenerator();

            modelBuilder.Entity<Token>().HasIndex(e => e.BookNumber);
            modelBuilder.Entity<Token>().HasIndex(e => e.ChapterNumber);
            modelBuilder.Entity<Token>().HasIndex(e => e.VerseNumber);

            //modelBuilder.Entity<Token>()
            //    .HasIndex(e => new { e.BookNumber, e.ChapterNumber, e.VerseNumber });

        }



        //public EntityEntry<TEntity> AddCopy<TEntity>(TEntity entity) where TEntity : class, new()
        //{
        //    Entry(entity).State = EntityState.Detached;
        //    var newEntity = CreateEntityCopy(entity);
        //    return Add(newEntity);
        //}

        //public async Task<EntityEntry<TEntity>> AddCopyAsync<TEntity>(TEntity entity) where TEntity : class, new()
        //{
        //    Entry(entity).State = EntityState.Detached;

        //    var e = (TEntity)Entry(entity).CurrentValues.ToObject();
        //    var newEntity = CreateEntityCopy(entity);
        //    return await AddAsync(newEntity);
        //}

        //private TEntity CreateCopy<TEntity>(TEntity entity)
        //{
        //    var json = JsonConvert.SerializeObject(entity);
        //    return JsonConvert.DeserializeObject<TEntity>(json);
        //}

        //private static TEntity CreateEntityCopy<TEntity>(TEntity entity) where TEntity : class, new()
        //{
        //    var newEntity = new TEntity();
        //    var propertyNamesToIgnore = new List<string> { "Id", "ParentId", "Created", "Modified" };
        //    var currentIdProperty = entity.GetType().GetProperty("Id");
        //    if (currentIdProperty != null)
        //    {
        //        var properties = entity.GetType().GetProperties()
        //            .Where(property => !propertyNamesToIgnore.Contains(property.Name));
        //        foreach (var propertyInfo in properties)
        //        {
        //            if (propertyInfo.PropertyType == typeof(ICollection<>))
        //            {

        //                var collectionObject = propertyInfo.GetValue(entity, null);
        //                var collection = Convert.ChangeType(collectionObject, propertyInfo.PropertyType);
        //                //foreach (var o in collection)
        //                //{
        //                //    //var e = CreateEntityCopy<TEntity>(o);
        //                //}

        //            }
        //            else
        //            {
        //                var property = propertyInfo.GetValue(entity, null);
        //                propertyInfo.SetValue(newEntity, property);
        //            }

        //        }

        //        var parentId = currentIdProperty.GetValue(entity, null);
        //        var parentIdProperty = entity.GetType().GetProperty("ParentId");
        //        parentIdProperty.SetValue(newEntity, parentId);
        //    }

        //    return newEntity;
        //}

        //public override EntityEntry<TEntity> Update<TEntity>(TEntity entity)
        //{



        //    return base.Update(entity);
        //}

        //public override EntityEntry Update(object entity)
        //{
        //    return base.Update(entity);
        //}

        //public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = new CancellationToken())
        //{
        //    return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        //}

        //public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        //{
        //    return base.SaveChangesAsync(cancellationToken);
        //}

        //public override int SaveChanges()
        //{
        //    foreach (var entityEntry in ChangeTracker.Entries()) // Iterate all made changes
        //    {
        //        var entityType = this.Model.FindEntityType(entityEntry.Entity.GetType());
        //        //if (entityEntry.State == EntityState.Modified) // If you want to update TenantId when Order is modified
        //        //{
        //        //    var entityType = this.Model.FindEntityType(entityEntry.Entity.GetType());
        //        //}
        //    }
        //    return base.SaveChanges();
        //}

    }
}
