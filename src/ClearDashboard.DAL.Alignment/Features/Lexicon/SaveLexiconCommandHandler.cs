using ClearBible.Engine.Utils;
using ClearDashboard.DAL.Alignment.Features.Common;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using Microsoft.Extensions.Logging;

//USE TO ACCESS Vocabulary
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.DAL.Alignment.Features.Lexicon
{
    public class SaveLexiconCommandHandler : ProjectDbContextCommandHandler<SaveLexiconCommand,
        RequestResult<IEnumerable<IId>>, IEnumerable<IId>>
    {
        private readonly IUserProvider _userProvider;
        public SaveLexiconCommandHandler(
            ProjectDbContextFactory? projectDbContextFactory,
            IProjectProvider projectProvider,
            IUserProvider userProvider,
            ILogger<SaveLexiconCommandHandler> logger) : base(projectDbContextFactory, projectProvider, logger)
        {
            _userProvider = userProvider;
        }

        protected override async Task<RequestResult<IEnumerable<IId>>> SaveDataAsync(SaveLexiconCommand request,
            CancellationToken cancellationToken)
        {
            var currentUserId = ModelHelper.BuildUserId(_userProvider.CurrentUser!);
            var currentDateTime = Models.TimestampedEntity.GetUtcNowRoundedToMillisecond();

            var createdIIds = new List<IId>();

            try
            {
                // ---------------------------------------------------------------------------------
                // Create / Update Lexemes and children:
                // ---------------------------------------------------------------------------------
                foreach (var lexeme in request.Lexicon.Lexemes)
                {
                    // ---------------------------------------------------------------------------------
                    // Create / Update Lexeme:
                    // ---------------------------------------------------------------------------------
                    if (!lexeme.IsInDatabase)
                    {
                        createdIIds.Add(
                            LexiconDataBuilder.CreateLexemeModel(ProjectDbContext, lexeme, currentDateTime, currentUserId)
                        );
                    }
                    else if (lexeme.IsDirty)
                    {
                        LexiconDataBuilder.UpdateLexemeModel(ProjectDbContext, lexeme);
                    }

                    // ---------------------------------------------------------------------------------
                    // Create and Update Lexeme Forms:
                    // ---------------------------------------------------------------------------------
                    foreach (var form in lexeme.Forms)
                    {
                        if (!form.IsInDatabase)
                        {
                            createdIIds.Add(
                                LexiconDataBuilder.CreateFormModel(ProjectDbContext, form, lexeme.LexemeId, currentDateTime, currentUserId)
                            );
                        }
                        else if (form.IsDirty)
                        {
                            LexiconDataBuilder.UpdateFormModel(ProjectDbContext, form, lexeme.LexemeId);
                        }
                    }

                    // ---------------------------------------------------------------------------------
                    // Delete Lexeme Forms:
                    // ---------------------------------------------------------------------------------
                    foreach (var formId in lexeme.FormIdsToDelete)
                    {
                        LexiconDataBuilder.DeleteFormModel(ProjectDbContext, formId);
                    }

                    // ---------------------------------------------------------------------------------
                    // Create / Update Lexeme Meanings and children:
                    // ---------------------------------------------------------------------------------
                    foreach (var meaning in lexeme.Meanings)
                    {
                        // ---------------------------------------------------------------------------------
                        // Create / Update Lexeme Meaning:
                        // ---------------------------------------------------------------------------------
                        if (!meaning.IsInDatabase)
                        {
                            createdIIds.Add(
                                LexiconDataBuilder.CreateMeaningModel(ProjectDbContext, meaning, lexeme.LexemeId, currentDateTime, currentUserId)
                            );
                        }
                        else if (meaning.IsDirty)
                        {
                            LexiconDataBuilder.UpdateMeaningModel(ProjectDbContext, meaning, lexeme.LexemeId);
                        }

                        // ---------------------------------------------------------------------------------
                        // Create / Update Lexeme Meaning Translations:
                        // ---------------------------------------------------------------------------------
                        foreach (var translation in meaning.Translations)
                        {
                            if (!translation.IsInDatabase)
                            {
                                createdIIds.Add(
                                    LexiconDataBuilder.CreateTranslationModel(ProjectDbContext, translation, meaning.MeaningId, currentDateTime, currentUserId)
                                );
                            }
                            else if (meaning.IsDirty)
                            {
                                LexiconDataBuilder.UpdateTranslationModel(ProjectDbContext, translation, meaning.MeaningId);
                            }
                        }

                        // ---------------------------------------------------------------------------------
                        // Delete Lexeme Meaning Translations:
                        // ---------------------------------------------------------------------------------
                        foreach (var translationId in meaning.TranslationIdsToDelete)
                        {
                            LexiconDataBuilder.DeleteTranslationModel(ProjectDbContext, translationId);
                        }
                    }

                    // ---------------------------------------------------------------------------------
                    // Delete Lexeme Meanings:
                    // ---------------------------------------------------------------------------------
                    foreach (var meaningId in lexeme.MeaningIdsToDelete)
                    {
                        LexiconDataBuilder.DeleteMeaningModel(ProjectDbContext, meaningId);
                    }
                }

                // ---------------------------------------------------------------------------------
                // Delete Lexemes:
                // ---------------------------------------------------------------------------------
                foreach (var lexemeId in request.Lexicon.LexemeIdsToDelete)
                {
                    LexiconDataBuilder.DeleteLexemeModel(ProjectDbContext, lexemeId);
                }

                _ = await ProjectDbContext!.SaveChangesAsync(cancellationToken);
                return new RequestResult<IEnumerable<IId>>(createdIIds);
            }
            catch (Exception ex)
            {
                return new RequestResult<IEnumerable<IId>>
                (
                    success: false,
                    message: ex.Message
                );
            }
        }
    }
}