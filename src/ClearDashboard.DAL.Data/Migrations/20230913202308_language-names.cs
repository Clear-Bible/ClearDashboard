using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Logging;
using SIL.WritingSystems;

#nullable disable

namespace ClearDashboard.DataAccessLayer.Data.Migrations
{
    /// <inheritdoc />
    public partial class languagenames : Migration
    {
        public static Dictionary<string, string>? languageNamesCodes = null;
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }

        public async static Task MigrateData(ProjectDbContext projectDbContext, ILogger logger, CancellationToken cancellationToken)
        {
            var appliedMigrations = projectDbContext.Database.GetAppliedMigrations();

            // If this migration hasn't yet been applied:
            var languageNamesMigrationId = GetMigrationAttributeId<languagenames>();
            if (!appliedMigrations.Contains(languageNamesMigrationId))
            {
                var dataChanged = false;

                if (Sldr.IsInitialized)
                {
                    Sldr.Cleanup();
                }
                Sldr.Initialize();
                var lookup = new LanguageLookup();

                // "InitialMigration" is where the Corpus table was created:
                var initialMigrationId = GetMigrationAttributeId<InitialMigration>();
                if (appliedMigrations.Contains(initialMigrationId))
                {
                    await projectDbContext.Corpa.ForEachAsync(e => 
                    {
                        e.Language = FindLanguageTag(lookup, e.Language, logger, nameof(Models.Corpus), ref dataChanged);
                    }, cancellationToken);
                }

                // "lexicon" is where Lexicon_Lexemes and Lexicon_Meanings were created:
                var lexiconMigrationId = GetMigrationAttributeId<lexicon>();
                if (appliedMigrations.Contains(lexiconMigrationId))
                {
                    await projectDbContext.Lexicon_Lexemes.ForEachAsync(e =>
                    {
                        e.Language = FindLanguageTag(lookup, e.Language, logger, nameof(Models.Lexicon_Lexeme), ref dataChanged);
                    }, cancellationToken);
                    await projectDbContext.Lexicon_Meanings.ForEachAsync(e =>
                    {
                        e.Language = FindLanguageTag(lookup, e.Language, logger, nameof(Models.Lexicon_Meaning), ref dataChanged);
                    }, cancellationToken);
                }

                if (dataChanged)
                {
                    await projectDbContext.SaveChangesAsync(cancellationToken);
                }
            }
        }

        private static string FindLanguageTag(LanguageLookup lookup, string languageName, ILogger logger, string tableName, ref bool dataChanged)
        {
            var suggestions = lookup.SuggestLanguages(languageName);
            if (suggestions != null && suggestions.Any())
            {
                var languageTag = suggestions.First().LanguageTag;
                if (languageTag != languageName)
                {
                    logger.LogInformation($"Converting language name '{languageName}' to tag '{languageTag}' in {tableName} table");
                    dataChanged = true;
                }
                return languageTag;
            }
            else
            {
                logger.LogInformation($"No suggested languages found for language name '{languageName}' from {tableName} table");
            }

            return languageName;
        }

        public static string GetMigrationAttributeId<T>()
        {
            var attribute = typeof(T).GetCustomAttributes(
                typeof(MigrationAttribute), true
            ).FirstOrDefault() as MigrationAttribute;

            if (attribute != null)
            {
                return attribute.Id;
            }
            return null;
        }
    }
}
