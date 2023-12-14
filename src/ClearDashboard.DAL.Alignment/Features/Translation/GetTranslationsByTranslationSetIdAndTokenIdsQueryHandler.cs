using ClearBible.Engine.Exceptions;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using ClearDashboard.DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

//USE TO ACCESS Models
using Models = ClearDashboard.DataAccessLayer.Models;
using OriginatedFromValues = ClearDashboard.DAL.Alignment.Translation.Translation.OriginatedFromValues;

namespace ClearDashboard.DAL.Alignment.Features.Translation
{
    public class GetTranslationsByTranslationSetIdAndTokenIdsQueryHandler : ProjectDbContextQueryHandler<
        GetTranslationsByTranslationSetIdAndTokenIdsQuery,
        RequestResult<IEnumerable<Alignment.Translation.Translation>>,
        IEnumerable<Alignment.Translation.Translation>>
    {
        public GetTranslationsByTranslationSetIdAndTokenIdsQueryHandler(
            ProjectDbContextFactory? projectNameDbContextFactory, 
            IProjectProvider projectProvider, 
            ILogger<GetTranslationsByTranslationSetIdAndTokenIdsQueryHandler> logger) : base(
                projectNameDbContextFactory, 
                projectProvider, 
                logger)
        {
        }

        protected override async Task<RequestResult<IEnumerable<Alignment.Translation.Translation>>> GetDataAsync(GetTranslationsByTranslationSetIdAndTokenIdsQuery request, CancellationToken cancellationToken)
        {
#if DEBUG
            Stopwatch sw = new();
            sw.Start();
            Logger.LogInformation($"Elapsed={sw.Elapsed} - Handler (start)");
#endif

            var translationSet = ProjectDbContext!.TranslationSets
                .Include(ts => ts.AlignmentSet)
                    .ThenInclude(ast => ast!.ParallelCorpus)
                .FirstOrDefault(ts => ts.Id == request.TranslationSetId.Id);

#if DEBUG
            sw.Stop();
#endif

            // need an await to get the compiler to be 'quiet'
            await Task.CompletedTask;

            if (translationSet == null)
            {
                return new RequestResult<IEnumerable<Alignment.Translation.Translation>>
                (
                    success: false,
                    message: $"Invalid TranslationSetId '{request.TranslationSetId.Id}' found in request"
                );
            }

#if DEBUG
            Logger.LogInformation($"Elapsed={sw.Elapsed} - Retrieve Translations '{translationSet.DisplayName}' (start)");
            sw.Restart();
#endif

            //var bookNumbers = request.TokenIds.GroupBy(t => t.BookNumber).Select(grp => grp.Key);
            var tokenIdGuids = request.TokenIds.Select(t => t.Id).ToList();

            var translations = await ModelHelper.AddIdIncludesTranslationsQuery(ProjectDbContext!)
                .Include(tr => tr.LexiconTranslation)
                    .ThenInclude(lt => lt != null ? lt.User : null)
                .Where(tr => tr.Deleted == null)
                .Where(tr => tr.TranslationSetId == request.TranslationSetId.Id)
                .Where(tr => tokenIdGuids.Contains(tr.SourceTokenComponentId))
                .Select(t => new Alignment.Translation.Translation(
                    ModelHelper.BuildTranslationId(t),
                    ModelHelper.BuildToken(t.SourceTokenComponent!),
                    t.TargetText ?? string.Empty,
                    t.TranslationState.ToString(),
                    t.LexiconTranslation != null ? ModelHelper.BuildTranslationId(t.LexiconTranslation) : null))
                .ToListAsync(cancellationToken);

            var tokenGuidsNotFound = tokenIdGuids.Except(translations.Select(t => t.SourceToken.TokenId.Id));

#if DEBUG
            sw.Stop();
            Logger.LogInformation($"Elapsed={sw.Elapsed} - Retrieve Translations '{translationSet.DisplayName}' (end)");
            sw.Restart();
#endif
            // For any token ids not found in Translations:
            if (tokenGuidsNotFound.Any())
            {
                var combined = await CombineModelTranslations(
                    translations, 
                    tokenGuidsNotFound, 
                    translationSet, 
                    request.TranslationSetId.ParallelCorpusId?.SourceTokenizedCorpusId?.CorpusId?.Language,
                    request.TranslationSetId.ParallelCorpusId?.TargetTokenizedCorpusId?.CorpusId?.Language,
                    request.AlignmentTypesToInclude,
                    cancellationToken);

#if DEBUG
                sw.Stop();
                Logger.LogInformation($"Elapsed={sw.Elapsed} - Combined Model Translations");
                sw.Restart();
#endif

                var tokensIdsNotFound = request.TokenIds
                    .Where(tid => !combined.Select(t => t.SourceToken.TokenId.Id).Contains(tid.Id))
                    .Select(tid => tid.Id)
                    .ToList();

#if DEBUG
                sw.Stop();
                Logger.LogInformation($"Elapsed={sw.Elapsed} - Calculate TokenIdsNotFound");
                sw.Restart();
#endif

                if (tokensIdsNotFound.Any())
                {
                    combined.AddRange(await ProjectDbContext.TokenComponents
                        .Include(tc => ((TokenComposite)tc).Tokens)
                        .Where(tc => tokensIdsNotFound.Contains(tc.Id))
                        .Select(tc => new Alignment.Translation.Translation(
                            ModelHelper.BuildToken(tc),
                            string.Empty,
                            OriginatedFromValues.FromAlignmentModel,
                            null)).ToListAsync(cancellationToken));
                    //                    throw new InvalidDataEngineException(name: "Token.Ids", value: $"{string.Join(",", tokenGuidsNotFound)}", message: "Token Ids not found in Translation Model");

#if DEBUG
                    sw.Stop();
                    Logger.LogInformation($"Elapsed={sw.Elapsed} - Added Translations for [{tokensIdsNotFound.Count}] token ids not found");
                    sw.Restart();
#endif
                }
#if DEBUG
                sw.Stop();
                Logger.LogInformation($"Elapsed={sw.Elapsed} - Handler (end)");
#endif

                return new RequestResult<IEnumerable<Alignment.Translation.Translation>>(
                    combined.OrderBy(t => t.SourceToken.TokenId.ToString()).ToList()
                );
            }
            else
            {
                return new RequestResult<IEnumerable<Alignment.Translation.Translation>>(translations.OrderBy(t => t.SourceToken.TokenId.ToString()).ToList());
            }
        }

        private async Task<List<Alignment.Translation.Translation>> CombineModelTranslations(
            IEnumerable<Alignment.Translation.Translation> translations, 
            IEnumerable<Guid> tokenGuidsNotFound, 
            Models.TranslationSet translationSet,
            string? sourceLanguage,
            string? targetLanguage,
            AlignmentTypes alignmentTypesToInclude,
            CancellationToken cancellationToken)
        {
            var combined = translations.ToList();

            // 1. Try TranslationModel if it exists:
            if (ProjectDbContext!.TranslationModelEntries
                .Where(tme => tme.TranslationSetId == translationSet.Id)
                .Any())
            {
#if DEBUG
                Stopwatch sw = new();
                sw.Start();
                Logger.LogInformation($"Elapsed={sw.Elapsed} - Add model translations (start)");
#endif

                var translationModelEntries = await ProjectDbContext!.TranslationModelEntries
                    .Include(tm => tm.TargetTextScores)
                    .Join(
                        ProjectDbContext!.TokenComponents
                            .Where(tc => tc.TokenizedCorpusId == translationSet.ParallelCorpus!.SourceTokenizedCorpusId)
                            .Where(tc => tokenGuidsNotFound.Contains(tc.Id)),
                        tm => tm.SourceText,
                        tc => tc.TrainingText ?? "",
                        (tm, tc) => new { tm, tc })
                    .Where(tmtc => tmtc.tm.TranslationSetId == translationSet.Id)
                    .Select(tmtc => new Alignment.Translation.Translation(
                        ModelHelper.BuildToken(tmtc.tc),
                        tmtc.tm.TargetTextScores.OrderByDescending(tts => tts.Score).First().Text ?? string.Empty,
                        OriginatedFromValues.FromTranslationModel,
                        null))
                    .ToListAsync(cancellationToken);

                combined.AddRange(translationModelEntries);

                tokenGuidsNotFound = tokenGuidsNotFound
                    .Where(tid => !combined.Select(t => t.SourceToken.TokenId.Id).Contains(tid))
                    .ToList();

#if DEBUG
                sw.Stop();
                Logger.LogInformation($"Elapsed={sw.Elapsed} - Retrieve TranslationModel '{translationSet.DisplayName}' (end)");
#endif
            }

            // 2.  Try Lexicon
            //  Here we first look for token training text values in Lexicon_Lexeme,
            //  and then for cases where there no matches, try Lexicon_Form:
            if (tokenGuidsNotFound.Any() && ProjectDbContext.Lexicon_Lexemes.Any())
            {
#if DEBUG
                Stopwatch sw = new();
                sw.Start();
                Logger.LogInformation($"Elapsed={sw.Elapsed} - Add lexicon translations (start)");
#endif

                // Query the database for 'not found' token ids:
                var sourceTokenComponentsByTrainingText = await GetSourceTokenComponentsByTrainingText(
                    tokenGuidsNotFound, 
                    translationSet.AlignmentSet!.ParallelCorpus!.SourceTokenizedCorpusId, 
                    cancellationToken);

                var translationsFromLexicon = new List<Alignment.Translation.Translation>();

                if (sourceTokenComponentsByTrainingText.Any())
                {
                    // Query the database for Lexemes that match language AND lemma (to any of the TokenComponents' TrainingText):
                    var lexemeMatches = (await ProjectDbContext.Lexicon_Lexemes
                        .Include(li => li.Meanings
                            .Where(m => string.IsNullOrEmpty(targetLanguage) || string.IsNullOrEmpty(m.Language) || m.Language == targetLanguage))
                            .ThenInclude(m => m.Translations)
                        .Where(li => li.Lemma != null)
                        .Where(li => string.IsNullOrEmpty(sourceLanguage) || string.IsNullOrEmpty(li.Language) || li.Language == sourceLanguage)
                        .Where(li => li.Meanings
                            .Where(m => string.IsNullOrEmpty(targetLanguage) || string.IsNullOrEmpty(m.Language) || m.Language == targetLanguage)
                            .Any(m => m.Translations.Any()))
                        .Where(li => sourceTokenComponentsByTrainingText.Keys.Contains(li.Lemma!))
                        .ToListAsync(cancellationToken))
                        .Select(li => (TrainingTextMatch: li.Lemma!, LexemeType: li.Type, FirstTranslation: li.Meanings
                            .SelectMany(lm => lm.Translations
                                .Select(lt => lt.Text!))
                            .First()))
                        .ToList();

                    translationsFromLexicon.AddRange(CreateTranslationsFromLexicon(lexemeMatches, sourceTokenComponentsByTrainingText));
                }

                // Any Lexeme matches from above result in values being removed from sourceTokenTrainingTexts.
                // If any are leftover, try forms:
                if (sourceTokenComponentsByTrainingText.Any())
                {
                    // Query the database for Forms that match Lexeme language AND form Text (to any of the TokenComponents' TrainingText):
                    var formMatches = (await ProjectDbContext.Lexicon_Forms
                        .Include(lf => lf.Lexeme)
                            .ThenInclude(li => li!.Meanings
                                .Where(m => string.IsNullOrEmpty(targetLanguage) || string.IsNullOrEmpty(m.Language) || m.Language == targetLanguage))
                                .ThenInclude(m => m.Translations)
                        .Where(lf => lf.Text != null)
                        .Where(lf => string.IsNullOrEmpty(sourceLanguage) || string.IsNullOrEmpty(lf.Lexeme!.Language) || lf.Lexeme!.Language == sourceLanguage)
                        .Where(lf => lf.Lexeme!.Meanings
                            .Where(m => string.IsNullOrEmpty(targetLanguage) || string.IsNullOrEmpty(m.Language) || m.Language == targetLanguage)
                            .Any(m => m.Translations.Any()))
                        .Where(lf => sourceTokenComponentsByTrainingText.Keys.Contains(lf.Text!))
                        .ToListAsync(cancellationToken))
                        .Select(lf => (TrainingTextMatch: lf.Text!, LexemeType: lf.Lexeme!.Type, FirstTranslation: lf.Lexeme!.Meanings
                            .SelectMany(lm => lm.Translations
                                .Select(lt => lt.Text!))
                            .First()))
                        .ToList();

                    translationsFromLexicon.AddRange(CreateTranslationsFromLexicon(formMatches, sourceTokenComponentsByTrainingText));
                }

                combined.AddRange(translationsFromLexicon);
                tokenGuidsNotFound = tokenGuidsNotFound.Except(translationsFromLexicon.Select(e => e.SourceToken.TokenId.Id)).ToList();

#if DEBUG
                sw.Stop();
                Logger.LogInformation($"Elapsed={sw.Elapsed} - Retrieve Translations from Lexicon '{translationSet.DisplayName}' (end)");
#endif
            }

            // 3.  For any leftovers, try the Alignments:
            if (tokenGuidsNotFound.Any())
            {
#if DEBUG
                Stopwatch sw = new();
                sw.Restart();
                Logger.LogInformation($"Elapsed={sw.Elapsed} - Add alignment translations (start)");
#endif

                if (ProjectDbContext!.AlignmentSetDenormalizationTasks.Any(a => a.AlignmentSetId == translationSet.AlignmentSetId))
                {
                    var sourceTokenComponentsByTrainingText = await GetSourceTokenComponentsByTrainingText(
                        tokenGuidsNotFound,
                        translationSet.AlignmentSet!.ParallelCorpus!.SourceTokenizedCorpusId,
                        cancellationToken);

                    var sourceTextToTopTargetTrainingText = (await ProjectDbContext!.Alignments
                        .Include(a => a.SourceTokenComponent)
                        .Include(a => a.TargetTokenComponent)
                        .Where(a => a.Deleted == null)
                        .Where(a => a.AlignmentSetId == translationSet.AlignmentSetId)
                        .Where(a => sourceTokenComponentsByTrainingText.Keys.Contains(a.SourceTokenComponent!.TrainingText!))
                        .ToListAsync(cancellationToken))
                        .WhereAlignmentTypesFilter(alignmentTypesToInclude)
                        .GroupBy(a => a.SourceTokenComponent!.TrainingText!)
                        .ToDictionary(g => g.Key, g => g
                            .GroupBy(a => a.TargetTokenComponent!.TrainingText)
                            .OrderByDescending(g => g.Count())
                            .First().Key);

                    combined.AddRange(sourceTextToTopTargetTrainingText
                        .SelectMany(kvp => sourceTokenComponentsByTrainingText[kvp.Key]
                            .Select(s =>
                                new Alignment.Translation.Translation(
                                        ModelHelper.BuildToken(s),
                                        kvp.Value!,
                                        OriginatedFromValues.FromAlignmentModel))
                            ));

#if DEBUG
                    sw.Stop();
                    Logger.LogInformation($"Elapsed={sw.Elapsed} - Retrieve Translations from Alignment model '{translationSet.DisplayName}' (end)");
#endif
                }
                else
                {
                    var translationsFromAlignmentModel = await ProjectDbContext.AlignmentTopTargetTrainingTexts
                        .Include(a => a.SourceTokenComponent!)
                            .ThenInclude(tc => ((TokenComposite)tc).Tokens)
                        .Where(a => a.AlignmentSetId == translationSet.AlignmentSetId)
                        .Where(a => tokenGuidsNotFound.Contains(a.SourceTokenComponentId))
                        .Select(a => new Alignment.Translation.Translation(
                            ModelHelper.BuildToken(a.SourceTokenComponent!),
                            a.TopTargetTrainingText,
                            OriginatedFromValues.FromAlignmentModel,
                            null))
                        .ToListAsync(cancellationToken);

                    combined.AddRange(translationsFromAlignmentModel);
#if DEBUG
                    sw.Stop();
                    Logger.LogInformation($"Elapsed={sw.Elapsed} - Retrieve [{translationsFromAlignmentModel.Count}] Translations from denormalized Alignment model '{translationSet.DisplayName}' (end)");
#endif
                }
            }

            return combined;
        }

        private async Task<IDictionary<string, IEnumerable<Models.TokenComponent>>> GetSourceTokenComponentsByTrainingText(
            IEnumerable<Guid> sourceTokenComponentIds,
            Guid sourceTokenizedCorpusId,
            CancellationToken cancellationToken)
        {
            var sourceTokenComponents = await ProjectDbContext!.TokenComponents
                .Include(tc => ((TokenComposite)tc).Tokens)
                .Where(tc => tc.TokenizedCorpusId == sourceTokenizedCorpusId)
                .Where(tc => sourceTokenComponentIds.Contains(tc.Id))
                .Where(tc => tc.TrainingText != null)
                .ToListAsync(cancellationToken);

            return sourceTokenComponents
                .GroupBy(tc => tc.TrainingText!)
                .ToDictionary(g => g.Key, g => g.Select(i => i));
        }

        private static IEnumerable<Alignment.Translation.Translation> CreateTranslationsFromLexicon(
            IEnumerable<(string TrainingTextMatch, string? LexemeType, string FirstTranslation)> lexemeOrFormMatches, 
            IDictionary<string, IEnumerable<Models.TokenComponent>> sourceTokensByTrainingText)
        {
            var translationsFromLexicon = new List<Alignment.Translation.Translation>();
            var translationSourceTokenIds = new List<Guid>();

            foreach (var (TrainingTextMatch, LexemeType, FirstTranslation) in lexemeOrFormMatches
                .OrderBy(e => e.TrainingTextMatch)
                .OrderByDescending(e => e.LexemeType))
            {
                foreach (var sourceToken in sourceTokensByTrainingText[TrainingTextMatch])
                {
                    if (!translationSourceTokenIds.Contains(sourceToken.Id) &&
                        (LexemeType is null || sourceToken.Type is null || LexemeType == sourceToken.Type))
                    {
                        translationsFromLexicon.Add(new Alignment.Translation.Translation(
                            ModelHelper.BuildToken(sourceToken),
                            FirstTranslation,
                            OriginatedFromValues.FromLexicon
                        ));

                        translationSourceTokenIds.Add(sourceToken.Id);
                    }
                }
            }

            // Remove token ids that were added to translations:
            foreach (var trainingText in sourceTokensByTrainingText.Keys)
            {
                sourceTokensByTrainingText[trainingText] = sourceTokensByTrainingText[trainingText]
                    .ExceptBy(translationSourceTokenIds, e => e.Id);
            }

            // Remove keys having empty values:
            foreach (var emptyItem in sourceTokensByTrainingText
                .Where(kvp => !kvp.Value.Any())
                .ToList())
            {
                sourceTokensByTrainingText.Remove(emptyItem.Key);
            }

            return translationsFromLexicon;
        }
    }
}
