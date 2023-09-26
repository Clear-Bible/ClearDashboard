using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Lexicon;
using ClearDashboard.DataAccessLayer.Data;
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.DAL.Alignment.Features.Common
{
    public static class LexiconDataBuilder
    {
        public static LexemeId CreateLexemeModel(
            ProjectDbContext projectDbContext,
            Alignment.Lexicon.Lexeme lexeme,
            DateTimeOffset defaultCreatedDate,
            UserId defaultUserId)
        {
            var lexemeDb = new Models.Lexicon_Lexeme
            {
                Id = lexeme.LexemeId.Id,
                Lemma = lexeme.Lemma,
                Type = lexeme.Type,
                Language = lexeme.Language,
                Created = lexeme.LexemeId.Created ?? defaultCreatedDate,
                UserId = lexeme.LexemeId.UserId?.Id ?? defaultUserId.Id
            };

            projectDbContext.Lexicon_Lexemes.Add(lexemeDb);

            return SimpleSynchronizableTimestampedEntityId<LexemeId>.Create(
                        lexeme.LexemeId.Id,
                        lexemeDb.Created,
                        lexeme.LexemeId.UserId ?? defaultUserId);
        }

        public static void UpdateLexemeModel(
            ProjectDbContext projectDbContext,
            Alignment.Lexicon.Lexeme lexeme)
        {
            var lexemeDb = LookupLexemeById(projectDbContext, lexeme.LexemeId);

            lexemeDb.Lemma = lexeme.Lemma;
            lexemeDb.Type = lexeme.Type;
            lexemeDb.Language = lexeme.Language;
        }

        public static void DeleteLexemeModel(ProjectDbContext projectDbContext, LexemeId lexemeId)
        {
            var lexemeDb = LookupLexemeById(projectDbContext, lexemeId);
            projectDbContext.Remove(lexemeDb);
        }

        private static Models.Lexicon_Lexeme LookupLexemeById(ProjectDbContext projectDbContext, LexemeId lexemeId)
        {
            var lexemeDb = projectDbContext.Lexicon_Lexemes.FirstOrDefault(l => l.Id == lexemeId.Id);
            return lexemeDb == null ? throw new Exception($"Invalid LexemeId.Id '{lexemeId.Id}' found in request") : lexemeDb;
        }

        public static FormId CreateFormModel(
            ProjectDbContext projectDbContext,
            Alignment.Lexicon.Form form,
            LexemeId lexemeId,
            DateTimeOffset defaultCreatedDate,
            UserId defaultUserId)
        {
            var formDb = new Models.Lexicon_Form
            {
                Id = form.FormId.Id,
                Text = form.Text,
                LexemeId = lexemeId.Id,
                Created = form.FormId.Created ?? defaultCreatedDate,
                UserId = form.FormId.UserId?.Id ?? defaultUserId.Id
            };

            projectDbContext.Lexicon_Forms.Add(formDb);

            return SimpleSynchronizableTimestampedEntityId<FormId>.Create(
                        form.FormId.Id,
                        defaultCreatedDate,
                        form.FormId.UserId ?? defaultUserId);
        }

        public static void UpdateFormModel(
            ProjectDbContext projectDbContext,
            Alignment.Lexicon.Form form,
            LexemeId lexemeId)
        {
            var formDb = LookupFormById(projectDbContext, form.FormId);

            formDb.Text = form.Text;
            formDb.LexemeId = lexemeId.Id;

            if (formDb.Lexeme is not null && formDb.LexemeId != formDb.Lexeme.Id)
            {
                formDb.Lexeme = null;
            }
        }

        public static void DeleteFormModel(ProjectDbContext projectDbContext, FormId formId)
        {
            var formDb = LookupFormById(projectDbContext, formId);
            projectDbContext.Remove(formDb);
        }

        public static Models.Lexicon_Form LookupFormById(ProjectDbContext projectDbContext, FormId formId)
        {
            var formDb = projectDbContext.Lexicon_Forms.FirstOrDefault(l => l.Id == formId.Id);
            return formDb == null ? throw new Exception($"Invalid FormId.Id '{formId.Id}' found in request") : formDb;
        }

        public static MeaningId CreateMeaningModel(
            ProjectDbContext projectDbContext,
            Alignment.Lexicon.Meaning meaning,
            LexemeId lexemeId,
            DateTimeOffset defaultCreatedDate,
            UserId defaultUserId)
        {
            var meaningDb = new Models.Lexicon_Meaning
            {
                Id = meaning.MeaningId.Id,
                Text = meaning.Text,
                Language = meaning.Language,
                LexemeId = lexemeId.Id,
                Created = meaning.MeaningId.Created ?? defaultCreatedDate,
                UserId = meaning.MeaningId.UserId?.Id ?? defaultUserId.Id
            };

            projectDbContext.Lexicon_Meanings.Add(meaningDb);

            return SimpleSynchronizableTimestampedEntityId<MeaningId>.Create(
                        meaning.MeaningId.Id,
                        defaultCreatedDate,
                        meaning.MeaningId.UserId ?? defaultUserId);
        }

        public static void UpdateMeaningModel(
            ProjectDbContext projectDbContext,
            Alignment.Lexicon.Meaning meaning,
            LexemeId lexemeId)
        {
            var meaningDb = LookupMeaningById(projectDbContext, meaning.MeaningId);

            meaningDb.Text = meaning.Text;
            meaningDb.Language = meaning.Language;
            meaningDb.LexemeId = lexemeId.Id;

            if (meaningDb.Lexeme is not null && meaningDb.LexemeId != meaningDb.Lexeme.Id)
            {
                meaningDb.Lexeme = null;
            }
        }

        public static void DeleteMeaningModel(ProjectDbContext projectDbContext, MeaningId meaningId)
        {
            var meaningDb = LookupMeaningById(projectDbContext, meaningId);
            projectDbContext.Remove(meaningDb);
        }

        private static Models.Lexicon_Meaning LookupMeaningById(ProjectDbContext projectDbContext, MeaningId meaningId)
        {
            var meaningDb = projectDbContext.Lexicon_Meanings.FirstOrDefault(l => l.Id == meaningId.Id);
            return meaningDb == null ? throw new Exception($"Invalid MeaningId.Id '{meaningId.Id}' found in request") : meaningDb;
        }

        public static TranslationId CreateTranslationModel(
            ProjectDbContext projectDbContext,
            Alignment.Lexicon.Translation translation,
            MeaningId meaningId,
            DateTimeOffset defaultCreatedDate,
            UserId defaultUserId)
        {
            var translationDb = new Models.Lexicon_Translation
            {
                Id = translation.TranslationId.Id,
                Text = translation.Text,
                MeaningId = meaningId.Id,
                Created = translation.TranslationId.Created ?? defaultCreatedDate,
                UserId = translation.TranslationId.UserId?.Id ?? defaultUserId.Id
            };

            projectDbContext.Lexicon_Translations.Add(translationDb);

            return SimpleSynchronizableTimestampedEntityId<TranslationId>.Create(
                        translation.TranslationId.Id,
                        defaultCreatedDate,
                        translation.TranslationId.UserId ?? defaultUserId);
        }

        public static void UpdateTranslationModel(
            ProjectDbContext projectDbContext,
            Alignment.Lexicon.Translation translation,
            MeaningId meaningId)
        {
            var translationDb = LookupTranslationById(projectDbContext, translation.TranslationId);

            translationDb.Text = translation.Text;
            translationDb.MeaningId = meaningId.Id;

            if (translationDb.Meaning is not null && translationDb.MeaningId != translationDb.Meaning.Id)
            {
                translationDb.Meaning = null;
            }
        }

        public static void DeleteTranslationModel(ProjectDbContext projectDbContext, TranslationId translationId)
        {
            var translationDb = LookupTranslationById(projectDbContext, translationId);
            projectDbContext.Remove(translationDb);
        }

        private static Models.Lexicon_Translation LookupTranslationById(ProjectDbContext projectDbContext, TranslationId translationId)
        {
            var translationDb = projectDbContext.Lexicon_Translations.FirstOrDefault(l => l.Id == translationId.Id);
            return translationDb == null ? throw new Exception($"Invalid TranslationId.Id '{translationId.Id}' found in request") : translationDb;
        }
    }
}
