using ClearBible.Engine.Exceptions;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using ClearDashboard.DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

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
            Stopwatch sw = new Stopwatch();
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

            var translations = ModelHelper.AddIdIncludesTranslationsQuery(ProjectDbContext!)
                .Include(tr => tr.LexiconTranslation)
                    .ThenInclude(lt => lt.User)
                .Where(tr => tr.Deleted == null)
                .Where(tr => tr.TranslationSetId == request.TranslationSetId.Id)
                .Where(tr => tokenIdGuids.Contains(tr.SourceTokenComponentId))
                .Select(t => new Alignment.Translation.Translation(
                    ModelHelper.BuildTranslationId(t),
                    ModelHelper.BuildToken(t.SourceTokenComponent!),
                    t.TargetText ?? string.Empty,
                    t.TranslationState.ToString(),
                    t.LexiconTranslation != null ? ModelHelper.BuildTranslationId(t.LexiconTranslation) : null))
                .ToList();

            var tokenGuidsNotFound = tokenIdGuids.Except(translations.Select(t => t.SourceToken.TokenId.Id));

#if DEBUG
            sw.Stop();
            Logger.LogInformation($"Elapsed={sw.Elapsed} - Retrieve Translations '{translationSet.DisplayName}' (end)");
            sw.Restart();
#endif
            // For any token ids not found in Translations:
            if (tokenGuidsNotFound.Any())
            {
                var combined = CombineModelTranslations(
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
                    combined.AddRange(ProjectDbContext.TokenComponents
                        .Include(tc => ((TokenComposite)tc).Tokens)
                        .Where(tc => tokensIdsNotFound.Contains(tc.Id))
                        .Select(tc => new Alignment.Translation.Translation(
                            ModelHelper.BuildToken(tc),
                            string.Empty,
                            OriginatedFromValues.FromAlignmentModel,
                            null)).ToList());
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

        private List<Alignment.Translation.Translation> CombineModelTranslations(
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
                .Where(tme => tme.TranslationSetId == translationSet.Id).Count() > 0)
            {
#if DEBUG
                Stopwatch sw = new Stopwatch();
                sw.Start();
                Logger.LogInformation($"Elapsed={sw.Elapsed} - Add model translations (start)");
#endif

                var translationModelEntries = ProjectDbContext!.TranslationModelEntries
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
                        null));

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
            if (tokenGuidsNotFound.Any() && ProjectDbContext.Lexicon_Lexemes.Any())
            {
#if DEBUG
                Stopwatch sw = new Stopwatch();
                sw.Start();
                Logger.LogInformation($"Elapsed={sw.Elapsed} - Add lexicon translations (start)");
#endif

                cancellationToken.ThrowIfCancellationRequested();

                var sourceTokenTrainingTexts = ProjectDbContext!.TokenComponents
                    .Include(tc => ((TokenComposite)tc).Tokens)
                    .Where(tc => tc.TokenizedCorpusId == translationSet.AlignmentSet!.ParallelCorpus!.SourceTokenizedCorpusId)
                    .Where(tc => tokenGuidsNotFound.Contains(tc.Id))
                    .Where(tc => tc.TrainingText != null)
                    .ToList()
                    .GroupBy(tc => tc.TrainingText!)
                    .ToDictionary(g => g.Key, g => g.Select(i => i));

                cancellationToken.ThrowIfCancellationRequested();

                combined.AddRange(ProjectDbContext.Lexicon_Lexemes
                    .Include(li => li.Meanings
                        .Where(d => string.IsNullOrEmpty(targetLanguage) || string.IsNullOrEmpty(d.Language) || d.Language == targetLanguage))
                    .Where(li => string.IsNullOrEmpty(sourceLanguage) || string.IsNullOrEmpty(li.Language) || li.Language == sourceLanguage)
                    .Where(li => sourceTokenTrainingTexts.Keys.Contains(li.Lemma))
                    .ToList()
                    .SelectMany(li => sourceTokenTrainingTexts[li.Lemma!]
                        .Select(t => new Alignment.Translation.Translation(
                            ModelHelper.BuildToken(t),
                            string.Join("/", li.Meanings.Select(lid => lid.Text)),
                            OriginatedFromValues.FromLexicon)
                        )
                    ));

                tokenGuidsNotFound = tokenGuidsNotFound
                    .Where(tid => !combined.Select(t => t.SourceToken.TokenId.Id).Contains(tid))
                    .ToList();

#if DEBUG
                sw.Stop();
                Logger.LogInformation($"Elapsed={sw.Elapsed} - Retrieve Translations from Lexicon '{translationSet.DisplayName}' (end)");
#endif
            }

            // 3.  For any leftovers, try the Alignments:
            if (tokenGuidsNotFound.Any())
            {
#if DEBUG
                Stopwatch sw = new Stopwatch();
                sw.Restart();
                Logger.LogInformation($"Elapsed={sw.Elapsed} - Add alignment translations (start)");
#endif

                if (ProjectDbContext!.AlignmentSetDenormalizationTasks.Any(a => a.AlignmentSetId == translationSet.AlignmentSetId))
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var sourceTokenTrainingTexts = ProjectDbContext!.TokenComponents
                        .Include(tc => ((TokenComposite)tc).Tokens)
                        .Where(tc => tc.TokenizedCorpusId == translationSet.AlignmentSet!.ParallelCorpus!.SourceTokenizedCorpusId)
                        .Where(tc => tokenGuidsNotFound.Contains(tc.Id))
                        .Where(tc => tc.TrainingText != null)
                        .ToList()
                        .GroupBy(tc => tc.TrainingText!)
                        .ToDictionary(g => g.Key, g => g.Select(i => i));

                    cancellationToken.ThrowIfCancellationRequested();

                    var sourceTextToTopTargetTrainingText = ProjectDbContext!.Alignments
                        .Include(a => a.SourceTokenComponent)
                        .Include(a => a.TargetTokenComponent)
                        .Where(a => a.Deleted == null)
                        .Where(a => a.AlignmentSetId == translationSet.AlignmentSetId)
                        .Where(a => sourceTokenTrainingTexts.Keys.Contains(a.SourceTokenComponent!.TrainingText))
                        .ToList()
                        .WhereAlignmentTypesFilter(alignmentTypesToInclude)
                        .GroupBy(a => a.SourceTokenComponent!.TrainingText!)
                        .ToDictionary(g => g.Key, g => g
                            .GroupBy(a => a.TargetTokenComponent!.TrainingText)
                            .OrderByDescending(g => g.Count())
                            .First().Key);

                    cancellationToken.ThrowIfCancellationRequested();

                    combined.AddRange(sourceTextToTopTargetTrainingText
                        .SelectMany(kvp => sourceTokenTrainingTexts[kvp.Key]
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
                    var translationsFromAlignmentModel = ProjectDbContext.AlignmentTopTargetTrainingTexts
                        .Include(a => a.SourceTokenComponent!)
                            .ThenInclude(tc => ((TokenComposite)tc).Tokens)
                        .Where(a => a.AlignmentSetId == translationSet.AlignmentSetId)
                        .Where(a => tokenGuidsNotFound.Contains(a.SourceTokenComponentId))
                        .Select(a => new Alignment.Translation.Translation(
                            ModelHelper.BuildToken(a.SourceTokenComponent!),
                            a.TopTargetTrainingText,
                            OriginatedFromValues.FromAlignmentModel,
                            null))
                        .ToList();

                    combined.AddRange(translationsFromAlignmentModel);
#if DEBUG
                    sw.Stop();
                    Logger.LogInformation($"Elapsed={sw.Elapsed} - Retrieve [{translationsFromAlignmentModel.Count}] Translations from denormalized Alignment model '{translationSet.DisplayName}' (end)");
#endif
                }
            }

            return combined;
        }
    }
}
