using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data.Extensions;
using ClearDashboard.DataAccessLayer.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Xml.Serialization;
// ReSharper disable InconsistentNaming

namespace ClearDashboard.DataAccessLayer.Data
{
    public class ProjectDbContext : DbContext
    {
        #nullable disable
        private readonly ILogger<ProjectDbContext> _logger;
        private readonly IMediator _mediator;
        public IUserProvider UserProvider { get; set; }
        public string DatabaseName { get; private set; }
        public DbContextOptionsBuilder<ProjectDbContext> OptionsBuilder { get; private set; }

        public ProjectDbContext(ILogger<ProjectDbContext> logger, IMediator mediator, IUserProvider userProvider, string databaseName, DbContextOptionsBuilder<ProjectDbContext> optionsBuilder)
            : base(optionsBuilder.Options)
        {
            _logger = logger;
            _mediator = mediator;
            UserProvider = userProvider;
            DatabaseName = databaseName;
            OptionsBuilder = optionsBuilder;
        }
        internal ProjectDbContext(string databaseName, DbContextOptionsBuilder<ProjectDbContext> optionsBuilder)
            : base(optionsBuilder.Options)
        {
            // This constructor is only for initial migration / design time usage
            DatabaseName = databaseName;
            OptionsBuilder = optionsBuilder;
        }
        protected ProjectDbContext(DbContextOptions options)
            : base(options)
        {

        }

        public virtual DbSet<Adornment> Adornments => Set<Adornment>();

        public virtual DbSet<AlignmentSet> AlignmentSets => Set<AlignmentSet>();
        public virtual DbSet<Alignment> Alignments => Set<Alignment>();

        public virtual DbSet<Corpus> Corpa => Set<Corpus>();
        public virtual DbSet<CorpusHistory> CorpaHistory => Set<CorpusHistory>();
        public virtual DbSet<Grammar> Grammars => Set<Grammar>();
        public virtual DbSet<NoteAssociation> NoteAssociations => Set<NoteAssociation>();
        public virtual DbSet<Note> Notes => Set<Note>();
        public virtual DbSet<Label> Labels => Set<Label>();
        public virtual DbSet<LabelGroup> LabelGroups => Set<LabelGroup>();
        public virtual DbSet<LabelGroupAssociation> LabelGroupAssociations => Set<LabelGroupAssociation>();
        public virtual DbSet<LabelNoteAssociation> LabelNoteAssociations => Set<LabelNoteAssociation>();
        public virtual DbSet<NoteDomainEntityAssociation> NoteDomainEntityAssociations => Set<NoteDomainEntityAssociation>();
        public virtual DbSet<NoteUserSeenAssociation> NoteUserSeenAssociations => Set<NoteUserSeenAssociation>();
        public virtual DbSet<ParallelCorpus> ParallelCorpa => Set<ParallelCorpus>();
        public virtual DbSet<ParallelCorpusHistory> ParallelCorpaHistory => Set<ParallelCorpusHistory>();
        public virtual DbSet<Project> Projects => Set<Project>();
        //public virtual DbSet<QuestionGroup> QuestionGroups => Set<QuestionGroup>();
        public virtual DbSet<RawContent> RawContent => Set<RawContent>();
        public virtual DbSet<Token> Tokens => Set<Token>();
        public virtual DbSet<TokenComponent> TokenComponents => Set<TokenComponent>();
        public virtual DbSet<TokenComposite> TokenComposites => Set<TokenComposite>();
        public virtual DbSet<TokenCompositeTokenAssociation> TokenCompositeTokenAssociations => Set<TokenCompositeTokenAssociation>();
        public virtual DbSet<TokenizedCorpus> TokenizedCorpora => Set<TokenizedCorpus>();
        public virtual DbSet<TokenVerseAssociation> TokenVerseAssociations => Set<TokenVerseAssociation>();
        public virtual DbSet<TranslationSet> TranslationSets => Set<TranslationSet>();
        public virtual DbSet<Translation> Translations => Set<Translation>();
        public virtual DbSet<TranslationModelEntry> TranslationModelEntries => Set<TranslationModelEntry>();
        public virtual DbSet<User> Users => Set<User>();
        public virtual DbSet<Verse> Verses => Set<Verse>();
        public virtual DbSet<VerseMapping> VerseMappings => Set<VerseMapping>();
        public virtual DbSet<VerseRow> VerseRows => Set<VerseRow>();
        //public virtual DbSet<VerseMappingVerseAssociation> VerseMappingVerseAssociations => Set<VerseMappingVerseAssociation>();

        //public virtual DbSet<ParallelTokenizedCorpus> ParallelTokenizedCorpa => Set<ParallelTokenizedCorpus>();

        public virtual DbSet<AlignmentSetDenormalizationTask> AlignmentSetDenormalizationTasks => Set<AlignmentSetDenormalizationTask>();
        public virtual DbSet<AlignmentTopTargetTrainingText> AlignmentTopTargetTrainingTexts => Set<AlignmentTopTargetTrainingText>();

        // =============
        // LEXICON:
        public virtual DbSet<Lexicon_Lexeme> Lexicon_Lexemes => Set<Lexicon_Lexeme>();
        public virtual DbSet<Lexicon_Meaning> Lexicon_Meanings => Set<Lexicon_Meaning>();
        public virtual DbSet<Lexicon_Form> Lexicon_Forms => Set<Lexicon_Form>();
        public virtual DbSet<Lexicon_Translation> Lexicon_Translations => Set<Lexicon_Translation>();
        public virtual DbSet<Lexicon_SemanticDomain> Lexicon_SemanticDomains => Set<Lexicon_SemanticDomain>();
        public virtual DbSet<Lexicon_SemanticDomainMeaningAssociation> Lexicon_SemanticDomainMeaningAssociations => Set<Lexicon_SemanticDomainMeaningAssociation>();
        // =============

        public async Task Migrate()
        {
            try
            {
                var whichOne = this.GetType().Name;

                // Ensure that the database is created.  Note that if we want to be able to apply migrations later,
                // we want to call Database.Migrate(), not Database.EnsureCreated().
                // https://stackoverflow.com/questions/38238043/how-and-where-to-call-database-ensurecreated-and-database-migrate
                _logger?.LogInformation("Ensuring that the database is created, migrating if necessary.");

                //todo comment out the if to allow migration for FileNew Projects
                //await Database.GetPendingMigrationsAsync();
                if ((await Database.GetPendingMigrationsAsync()).Any())
                {
                    if (whichOne != nameof(NpgsqlProjectDbContext))
                    {
                        // Should convert languageNames to languageTags in database,
                        // if it hasn't been done already:
                        await Migrations.languagenames.MigrateData(this, _logger, CancellationToken.None);
                    }

                    _logger?.LogInformation("Migration required, migrating...");
                    await Database.MigrateAsync();
                    //await Database.CloseConnectionAsync();

                    await EnsureMigrated();
                }
                _logger?.LogInformation("Check to migrate database complete");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Could not apply database migrations");

                // This is useful when applying migrations via the EF core plumbing, please leave in place.
                Console.WriteLine($"Could not apply database migrations: {ex.Message}");
            }
        }

        private async Task EnsureMigrated()
        {
            try
            {
                _ = Projects.ToList();
            }
            catch
            {
                _logger.LogInformation($"The migrations for the {DatabaseName} database failed -- forcing the migrations again.");
                await Database.MigrateAsync();
            }
        }


        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{

        //    optionsBuilder.EnableSensitiveDataLogging();

        //    if (_logger != null)
        //    {
        //        optionsBuilder.LogTo(
        //            s =>
        //            {
        //                //if (LoggingEnabled)
        //                //{
        //                _logger.LogInformation(s);
        //                Console.WriteLine(s);
        //                //}
        //            }, LogLevel.Information);
        //    }

        //    base.OnConfiguring(optionsBuilder);
        //}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // We want our table names to be singular
            modelBuilder.RemovePluralizingTableNameConvention(this);

            modelBuilder.Entity<Alignment>()
                .HasOne(e => e.SourceTokenComponent)
                .WithMany(e => e.SourceAlignments);

            modelBuilder.Entity<Alignment>()
                .HasOne(e => e.TargetTokenComponent)
                .WithMany(e => e.TargetAlignments);

            modelBuilder.Entity<AlignmentSet>()
                .Property(e => e.Metadata)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, default(JsonSerializerOptions)),
                    v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, default(JsonSerializerOptions)) ?? new Dictionary<string, object>(),
                    new ValueComparer<Dictionary<string, object>>(
                        (c1, c2) => c1.SequenceEqual(c2!),
                        c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                        c => c));

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

            modelBuilder.Entity<Grammar>().HasAlternateKey(e => e.ShortName);
            modelBuilder.Entity<Grammar>().HasIndex(e => e.ShortName).IsUnique();
            

            modelBuilder.Entity<ParallelCorpus>()
               .Property(e => e.Metadata)
               .HasConversion(
                   v => JsonSerializer.Serialize(v, default(JsonSerializerOptions)),
                   v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, default(JsonSerializerOptions)) ?? new Dictionary<string, object>(),
                   new ValueComparer<Dictionary<string, object>>(
                       (c1, c2) => c1.SequenceEqual(c2!),
                       c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                       c => c));

            modelBuilder.Entity<ParallelCorpusHistory>()
               .Property(e => e.Metadata)
               .HasConversion(
                   v => JsonSerializer.Serialize(v, default(JsonSerializerOptions)),
                   v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, default(JsonSerializerOptions)) ?? new Dictionary<string, object>(),
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

            modelBuilder.Entity<TranslationSet>()
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

            modelBuilder.Entity<ParallelCorpus>()
                .HasMany(pc => pc.VerseMappings)
                .WithOne(vm => vm.ParallelCorpus)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<VerseMapping>()
                .HasMany(vm => vm.Verses)
                .WithOne(v => v.VerseMapping)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Verse>()
                .HasMany(v => v.TokenVerseAssociations)
                .WithOne(tva => tva.Verse)
                .OnDelete(DeleteBehavior.Cascade);

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

            var tokenComponentTableName = modelBuilder.Model.FindEntityType(typeof(TokenComponent)).GetTableName();
            modelBuilder.Entity<Token>().ToTable(tokenComponentTableName);
            modelBuilder.Entity<TokenComposite>().ToTable(tokenComponentTableName);
            //modelBuilder.Entity<TokenComposite>().Navigation(e => e.Tokens).AutoInclude();

            modelBuilder
                .Entity<TokenComposite>()
                .HasMany(p => p.Tokens)
                .WithMany(p => p.TokenComposites)
                .UsingEntity<TokenCompositeTokenAssociation>();

            modelBuilder.Entity<VerseRow>()
                .HasMany(e => e.TokenComponents)
                .WithOne(e => e.VerseRow)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<VerseRow>().HasIndex(e => e.BookChapterVerse);

            modelBuilder.Entity<TokenComponent>().HasIndex(e => e.EngineTokenId);
            modelBuilder.Entity<TokenComponent>().HasIndex(e => e.TokenizedCorpusId);
            modelBuilder.Entity<TokenComponent>().HasIndex(e => e.TrainingText);
            modelBuilder.Entity<TokenComponent>().HasIndex(e => e.SurfaceText);
            modelBuilder.Entity<TokenComponent>().HasIndex(e => e.GrammarId);
            modelBuilder.Entity<TokenComponent>().HasIndex(e => e.CircumfixGroup);
            modelBuilder.Entity<TokenComponent>().HasIndex(e => e.Type);

            // Configure the TokenComponent.Metadata property as a JSON column
            modelBuilder.Entity<TokenComponent>().OwnsMany(tokenComponent => tokenComponent.Metadata, ownedNavigationBuilder =>
            {
                ownedNavigationBuilder.ToJson(); // Store as JSON
            });


            modelBuilder.Entity<Token>().HasIndex(e => e.OriginTokenLocation);
            modelBuilder.Entity<Token>().HasIndex(e => e.BookNumber);
            modelBuilder.Entity<Token>().HasIndex(e => e.ChapterNumber);
            modelBuilder.Entity<Token>().HasIndex(e => e.VerseNumber);
            //modelBuilder.Entity<Token>().HasIndex(e => e.WordNumber);
            //modelBuilder.Entity<Token>().HasIndex(e => e.SubwordNumber);

          

            modelBuilder.Entity<TokenCompositeTokenAssociation>().HasIndex(e => e.TokenId);
            modelBuilder.Entity<TokenCompositeTokenAssociation>().HasIndex(e => e.TokenCompositeId);

            //modelBuilder.Entity<Token>()
            //    .HasIndex(e => new { e.BookNumber, e.ChapterNumber, e.VerseNumber });

//            modelBuilder.Entity<Translation>().Navigation(e => e.SourceTokenComponent).AutoInclude();
            modelBuilder.Entity<TranslationModelEntry>()
                .HasIndex(e => new { e.TranslationSetId, e.SourceText })
                .IsUnique()
                .HasDatabaseName("IX_Trans_Model_Entry_Trans_Set_Id_Source_Text");
            modelBuilder.Entity<TranslationModelTargetTextScore>()
                .HasIndex(e => new { e.TranslationModelEntryId, e.Text})
                .IsUnique()
                .HasDatabaseName("IX_Trans_Model_Score_Entry_Id_Text");

            modelBuilder.Entity<Label>()
                .HasIndex(e => e.Text)
                .IsUnique();

            modelBuilder.Entity<LabelGroup>()
                .HasIndex(e => e.Name)
                .IsUnique();

            modelBuilder
                .Entity<LabelGroup>()
                .HasMany(p => p.Labels)
                .WithMany(p => p.LabelGroups)
                .UsingEntity<LabelGroupAssociation>();

            modelBuilder
                .Entity<Note>()
                .HasMany(p => p.Labels)
                .WithMany(p => p.Notes)
                .UsingEntity<LabelNoteAssociation>();

            modelBuilder.Entity<Verse>().HasIndex(e => e.BookNumber);
            modelBuilder.Entity<Verse>().HasIndex(e => e.ChapterNumber);
            modelBuilder.Entity<Verse>().HasIndex(e => e.VerseNumber);
            modelBuilder.Entity<Verse>().HasIndex(e => e.BBBCCCVVV);

            modelBuilder.Entity<AlignmentTopTargetTrainingText>()
                .HasIndex(e => new { e.AlignmentSetId, e.SourceTokenComponentId })
                .HasDatabaseName("IX_Align_Top_Training_Align_Set_Id_Source_Token_Id");

            modelBuilder.Entity<AlignmentTopTargetTrainingText>()
                .HasIndex(e => e.SourceTokenComponentId)
                .HasDatabaseName("IX_Align_Top_Training_Source_Token_Id");

            modelBuilder.Entity<AlignmentTopTargetTrainingText>()
                .HasIndex(e => new { e.AlignmentSetId, e.SourceTrainingText })
                .HasDatabaseName("IX_Align_Top_Training_Align_Set_Id_Source_Training");

            // =============
            // LEXICON:
            modelBuilder
                .Entity<Lexicon_SemanticDomain>()
                .HasMany(e => e.Meanings)
                .WithMany(e => e.SemanticDomains)
                .UsingEntity<Lexicon_SemanticDomainMeaningAssociation>();

            modelBuilder.Entity<Lexicon_Lexeme>()
                .HasIndex(e => new { e.Lemma, e.Type, e.Language })
                .HasFilter("Type IS NOT NULL")
                .IsUnique()
                .HasDatabaseName("IX_Lexicon_Lexeme_Lemma_TypeNotNull_Language");

            modelBuilder.Entity<Lexicon_Lexeme>()
                .HasIndex(e => new { e.Lemma, e.Language })
                .HasFilter("Type IS NULL")
                .IsUnique()
                .HasDatabaseName("IX_Lexicon_Lexeme_Lemma_TypeNull_Language");

            modelBuilder.Entity<Lexicon_Form>()
                .HasIndex(e => e.Text);
            // =============
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

        public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = new CancellationToken())
        {
            await _mediator.DispatchDomainEventsAsync(this, cancellationToken);
            return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

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
