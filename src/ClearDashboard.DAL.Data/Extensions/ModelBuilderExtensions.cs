using ClearDashboard.DataAccessLayer.Data.EntityConfiguration;
using ClearDashboard.DataAccessLayer.Data.ValueGenerators;
using ClearDashboard.DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using StringContent = ClearDashboard.DataAccessLayer.Models.StringContent;

namespace ClearDashboard.DataAccessLayer.Data.Extensions
{
    public static class ModelBuilderExtensions
    {

        public static void AddUserIdValueGenerator(this ModelBuilder modelBuilder)
        {
            var entitiesToIgnore = new List<string> { "User", "NoteRecipient" };
            foreach (var entityType in modelBuilder.Model.GetEntityTypes().Where(entityType=>!entitiesToIgnore.Contains(entityType.Name)))
            {
                var userIdProperty = entityType.ClrType.GetProperties().FirstOrDefault(p => p.Name=="UserId" && (p.PropertyType == typeof(Guid) || p.PropertyType == typeof(Guid?)));
                if (userIdProperty != null)
                {
                    modelBuilder
                        .Entity(entityType.Name)
                        .Property(userIdProperty.Name)
                        .HasValueGenerator<UserIdValueGenerator>();
                }

            }
        }

        public static void AddDateTimeOffsetToBinaryConverter(this ModelBuilder modelBuilder, string? databaseProviderName = null)
        {
            if (databaseProviderName is "Microsoft.EntityFrameworkCore.Sqlite")
            {
                // SQLite does not have proper support for DateTimeOffset via Entity Framework Core, see the limitations
                // here: https://docs.microsoft.com/en-us/ef/core/providers/sqlite/limitations#query-limitations
                // To work around this, when the Sqlite database provider is used, all model properties of type DateTimeOffset
                // use the DateTimeOffsetToBinaryConverter
                // Based on: https://github.com/aspnet/EntityFrameworkCore/issues/10784#issuecomment-415769754
                // This only supports millisecond precision, but should be sufficient for most use cases.
                foreach (var entityType in modelBuilder.Model.GetEntityTypes())
                {
                    var properties = entityType.ClrType.GetProperties().Where(p => p.PropertyType == typeof(DateTimeOffset)
                        || p.PropertyType ==
                        typeof(DateTimeOffset?));
                    foreach (var property in properties)
                    {
                        modelBuilder
                            .Entity(entityType.Name)
                            .Property(property.Name)
                            .HasConversion(new DateTimeOffsetToBinaryConverter());
                    }
                }
            }
        }

        /// <summary>
        /// Remove pluralizing table name convention to create table name in singular form.
        /// </summary>       
        public static void RemovePluralizingTableNameConvention(this ModelBuilder modelBuilder)
        {
            foreach (IMutableEntityType entityType in modelBuilder.Model.GetEntityTypes())
            {
                entityType.SetTableName(entityType.DisplayName());
            }
        }

        public static void ConfigureEntities(this ModelBuilder modelBuilder)
        {
            new AdornmentConfiguration().Configure(modelBuilder.Entity<Adornment>());
            new AlignmentConfiguration().Configure(modelBuilder.Entity<Alignment>());
            new AlignmentVersionConfiguration().Configure(modelBuilder.Entity<AlignmentVersion>());
            new CorpusConfiguration().Configure(modelBuilder.Entity<Corpus>());
            //new DataAssociationConfiguration().Configure(modelBuilder.Entity<DataAssociation>());
            //new InterlinearNoteConfiguration().Configure(modelBuilder.Entity<InterlinearNote>());
            //new NoteConfiguration().Configure(modelBuilder.Entity<Note>());
            new ParallelCorpusConfiguration().Configure(modelBuilder.Entity<ParallelCorpus>());
            //new ParallelVersesLinkConfiguration().Configure(modelBuilder.Entity<ParallelVersesLink>());
            new ProjectInfoConfiguration().Configure(modelBuilder.Entity<Project>());
            //new QuestionGroupConfiguration().Configure(modelBuilder.Entity<QuestionGroup>());
            //new RawContentConfiguration().Configure(modelBuilder.Entity<RawContent>());
            new TokenConfiguration().Configure(modelBuilder.Entity<Token>());
            new UserConfiguration().Configure(modelBuilder.Entity<User>());
            new VerseConfiguration().Configure(modelBuilder.Entity<Verse>());
            //new VerseLinkConfiguration().Configure(modelBuilder.Entity<VerseLink>());
        }

        public static void ConfigureRawContentEntities(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RawContent>()
                .HasDiscriminator(entity => entity.ContentType);

            modelBuilder.Entity<StringContent>();
            modelBuilder.Entity<BinaryContent>();
        }

        public static void ConfigureNoteAssociationEntities(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<NoteAssociation>()
                .HasDiscriminator(entity => entity.AssociationType);

            modelBuilder.Entity<AlignmentAssociation>();
            modelBuilder.Entity<BookAssociation>();
            modelBuilder.Entity<ChapterAssociation>();
            modelBuilder.Entity<TokenAssociation>();
            modelBuilder.Entity<VerseAssociation>();

        }
    }
}
