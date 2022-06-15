using System.Reflection;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data.EntityConfiguration;
using ClearDashboard.DataAccessLayer.Data.Extensions;
using ClearDashboard.DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;


namespace ClearDashboard.DataAccessLayer.Data
{
    public class AlignmentContext : DbContext
    {
        private readonly ILogger<AlignmentContext>? _logger;
        public readonly IUserProvider UserProvider;

        public AlignmentContext() : this(string.Empty)
        {
           
        }

        public AlignmentContext(ILogger<AlignmentContext> logger, IUserProvider userProvider) :  this(string.Empty)
        {
            _logger = logger;
            UserProvider = userProvider;
        }

        public AlignmentContext(DbContextOptions<AlignmentContext> options)
            : base(options)
        {
            DatabasePath = string.Empty;
        }

        protected AlignmentContext(string databasePath)
        {
            DatabasePath = databasePath;
        }

        public virtual DbSet<Adornment> Adornments => Set<Adornment>();
        public virtual DbSet<Alignment> Alignments => Set<Alignment>();

        public virtual DbSet<AlignmentSet> AlignmentSets => Set<AlignmentSet>();
        public virtual DbSet<AlignmentTokenPair> AlignmentTokenPairs => Set<AlignmentTokenPair>();

        public virtual DbSet<AlignmentVersion> AlignmentVersions => Set<AlignmentVersion>();
        public virtual DbSet<Corpus> Corpa => Set<Corpus>();
        public virtual DbSet<CorpusVersion> CorpaVersions => Set<CorpusVersion>();
        public virtual DbSet<NoteAssociation> DataAssociations => Set<NoteAssociation>();
        //public virtual DbSet<InterlinearNote> InterlinearNotes => Set<InterlinearNote>();
        public virtual DbSet<Note> Notes => Set<Note>();
        public virtual DbSet<ParallelCorpus> ParallelCorpa => Set<ParallelCorpus>();
        public virtual DbSet<ParallelCorpusVersion> ParallelCorpaVersions => Set<ParallelCorpusVersion>();
        //public virtual DbSet<ParallelVersesLink> ParallelVersesLinks => Set<ParallelVersesLink>();
        public virtual DbSet<ProjectInfo> ProjectInfos => Set<ProjectInfo>();
        public virtual DbSet<QuestionGroup> QuestionGroups => Set<QuestionGroup>();
        public virtual DbSet<RawContent> RawContent => Set<RawContent>();
        public virtual DbSet<Token> Tokens => Set<Token>();
        public virtual DbSet<Tokenization> Tokenizations => Set<Tokenization>();
        public virtual DbSet<User> Users => Set<User>();
        public virtual DbSet<Verse> Verses => Set<Verse>();
        public virtual DbSet<VerseMapping> VerseMappings => Set<VerseMapping>();
        public virtual DbSet<VerseMappingVerseAssociation> VerseMappingVerseAssociations => Set<VerseMappingVerseAssociation>();
        public virtual DbSet<VerseMappingTokenizationsAssociation> VerseMappingTokenizationsAssociations => Set<VerseMappingTokenizationsAssociation>();

       // public virtual DbSet<VerseLink> VerseLinks => Set<VerseLink>();


        public string DatabasePath { get; set; }

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
            var newEntity = CreateEntityCopy(entity);
            return await AddAsync(newEntity);
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
                    var property = propertyInfo.GetValue(entity, null);
                    propertyInfo.SetValue(newEntity, property);
                }

                var parentId = currentIdProperty.GetValue(entity, null);
                var parentIdProperty = entity.GetType().GetProperty("ParentId");
                parentIdProperty.SetValue(newEntity, parentId);
            }

            return newEntity;
        }

        public override EntityEntry<TEntity> Update<TEntity>(TEntity entity)
        {
           


            return base.Update(entity);
        }

        public override EntityEntry Update(object entity)
        {
            return base.Update(entity);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = new CancellationToken())
        {
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            return base.SaveChangesAsync(cancellationToken);
        }

        public override int SaveChanges()
        {
            foreach (var entityEntry in ChangeTracker.Entries()) // Iterate all made changes
            {
                var entityType = this.Model.FindEntityType(entityEntry.Entity.GetType());
                //if (entityEntry.State == EntityState.Modified) // If you want to update TenantId when Order is modified
                //{
                //    var entityType = this.Model.FindEntityType(entityEntry.Entity.GetType());
                //}
            }
            return base.SaveChanges();
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // We want our table names to be singular
            modelBuilder.RemovePluralizingTableNameConvention();

            // NB:  I'm relying on the default naming and relationship conventions from EF Core to set up the database...

            //  **** leaving this here in the event we need to override the default conventions ****
            // NB:  Add the configuration of any newly added 
            //      entities to the ConfigureEntities extension method
            //modelBuilder.ConfigureEntities();

            modelBuilder.Entity<AlignmentTokenPair>()
                .HasOne(e => e.SourceToken)
                .WithMany(e=>e.SourceAlignmentTokenPairs);
              

            modelBuilder.Entity<AlignmentTokenPair>()
                .HasOne(e => e.TargetToken)
                .WithMany(e=>e.TargetAlignmentTokenPairs);


            modelBuilder.Entity<ParallelCorpusVersion>()
                .HasOne(e => e.SourceCorpus)
                .WithMany(e => e.SourceParallelCorpusVersions);


            modelBuilder.Entity<ParallelCorpusVersion>()
                .HasOne(e => e.TargetCorpus)
                .WithMany(e => e.TargetParallelCorpusVersions);

          
            modelBuilder.Entity<Tokenization>()
                .HasMany(e => e.VerseMappingTokenizationsAssociations)
                .WithOne();

           
            // NB:  Add any new entities which inherit from RawContent
            //      to the ConfigureRawContentEntities extension method
            modelBuilder.ConfigureRawContentEntities();

            // NB:  Add any new entities which inherit from NoteAssociation
            //      to the ConfigureNoteAssociationEntities extension method
            modelBuilder.ConfigureNoteAssociationEntities();

            // now handle any entities which have DatetimeOffset properties.                                                  
            modelBuilder.AddDateTimeOffsetToBinaryConverter(Database.ProviderName);
            modelBuilder.AddUserIdValueGenerator();

        }
    }
}
