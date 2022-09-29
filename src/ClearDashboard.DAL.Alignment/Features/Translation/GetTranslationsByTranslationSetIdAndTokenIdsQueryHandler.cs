using ClearBible.Engine.Exceptions;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using ClearDashboard.DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace ClearDashboard.DAL.Alignment.Features.Translation
{
    public class GetTranslationsByTranslationSetIdAndTokenIdsQueryHandler : ProjectDbContextQueryHandler<
        GetTranslationsByTranslationSetIdAndTokenIdsQuery,
        RequestResult<IEnumerable<Alignment.Translation.Translation>>,
        IEnumerable<Alignment.Translation.Translation>>
    {

        public GetTranslationsByTranslationSetIdAndTokenIdsQueryHandler(ProjectDbContextFactory? projectNameDbContextFactory, IProjectProvider projectProvider, ILogger<GetTranslationsByTranslationSetIdAndTokenIdsQueryHandler> logger) 
            : base(projectNameDbContextFactory, projectProvider, logger)
        {
        }

        protected override async Task<RequestResult<IEnumerable<Alignment.Translation.Translation>>> GetDataAsync(GetTranslationsByTranslationSetIdAndTokenIdsQuery request, CancellationToken cancellationToken)
        {
            var translationSet = ProjectDbContext!.TranslationSets
                .Include(ts => ts.AlignmentSet)
                    .ThenInclude(ast => ast!.ParallelCorpus)
                .FirstOrDefault(ts => ts.Id == request.TranslationSetId.Id);

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

            //var bookNumbers = request.TokenIds.GroupBy(t => t.BookNumber).Select(grp => grp.Key);
            var tokenIdGuids = request.TokenIds.Select(t => t.Id).ToList();

            var translations = ProjectDbContext!.Translations
                .Where(tr => tr.TranslationSetId == request.TranslationSetId.Id)
                .Where(tr => tokenIdGuids.Contains(tr.SourceTokenComponent!.Id))
                .Select(t => new Alignment.Translation.Translation(
                    ModelHelper.BuildToken(t.SourceTokenComponent!),
                    t.TargetText ?? string.Empty,
                    t.TranslationState.ToString()));

            var tokenGuidsNotFound = tokenIdGuids.Except(translations.Select(t => t.SourceToken.TokenId.Id));

            // For any token ids not found in Translations:
            if (tokenGuidsNotFound.Any())
            {
                var combined = translations.ToList();

                if (ProjectDbContext!.TranslationModelEntries
                    .Where(tme => tme.TranslationSetId == request.TranslationSetId.Id).Count() > 0)
                {
                    var translationModelEntries = ProjectDbContext!.TranslationModelEntries
                         .Include(tm => tm.TargetTextScores)
                         .Join(
                             ProjectDbContext!.TokenComponents
                                 .Where(tc => tc.TokenizationId == translationSet.ParallelCorpus!.SourceTokenizedCorpusId)
                                 .Where(tc => tokenGuidsNotFound.Contains(tc.Id)),
                             tm => tm.SourceText,
                             tc => tc.TrainingText ?? "",
                             (tm, tc) => new { tm, tc })
                         .Where(tmtc => tmtc.tm.TranslationSetId == request.TranslationSetId.Id)
                         .Select(tmtc => new Alignment.Translation.Translation(
                             ModelHelper.BuildToken(tmtc.tc),
                             tmtc.tm.TargetTextScores.OrderByDescending(tts => tts.Score).First().Text ?? string.Empty,
                             "FromTranslationModel"));

                    combined.AddRange(translationModelEntries);
                } 
                else
                {
                    var sourceTokenTrainingTexts = ProjectDbContext!.TokenComponents
                        .Where(tc => tc.TokenizationId == translationSet.AlignmentSet!.ParallelCorpus!.SourceTokenizedCorpusId)
                        .Where(tc => tokenGuidsNotFound.Contains(tc.Id))
                        .Where(tc => tc.TrainingText != null)
                        .ToList()
                        .GroupBy(tc => tc.TrainingText!)
                        .ToDictionary(g => g.Key, g => g.Select(i => i));

                    var sourceTextToTopTargetTrainingText = ProjectDbContext!.Alignments
                        .Include(a => a.SourceTokenComponent)
                        .Include(a => a.TargetTokenComponent)
                        .Where(a => a.AlignmentSetId == translationSet.AlignmentSetId)
                        .Where(a => sourceTokenTrainingTexts.Keys.Contains(a.SourceTokenComponent!.TrainingText))
                        .ToList()
                        .GroupBy(a => a.SourceTokenComponent!.TrainingText!)
                        .ToDictionary(g => g.Key, g => g
                            .GroupBy(a => a.TargetTokenComponent!.TrainingText)
                            .OrderByDescending(g => g.Count())
                            .First().Key);

                    combined.AddRange(sourceTextToTopTargetTrainingText
                        .SelectMany(kvp => sourceTokenTrainingTexts[kvp.Key]
                            .Select(s =>
                                new Alignment.Translation.Translation(
                                        ModelHelper.BuildToken(s),
                                        kvp.Value,
                                        "FromAlignmentModel"))
                            ));
                }

                var tokensIdsNotFound = request.TokenIds
                    .Where(tid => !combined.Select(t => t.SourceToken.TokenId.Id).Contains(tid.Id))
                    .Select(tid => tid.Id)
                    .ToList();

                if (tokensIdsNotFound.Any())
                {
                    combined.AddRange(ProjectDbContext.TokenComponents
                        .Where(tc => tokensIdsNotFound.Contains(tc.Id))
                        .Select(tc => new Alignment.Translation.Translation(
                            ModelHelper.BuildToken(tc),
                            null,
                            "FromAlignmentModel")));
//                    throw new InvalidDataEngineException(name: "Token.Ids", value: $"{string.Join(",", tokenGuidsNotFound)}", message: "Token Ids not found in Translation Model");
                }

                return new RequestResult<IEnumerable<Alignment.Translation.Translation>>(
                    combined.OrderBy(t => t.SourceToken.TokenId.ToString())
                );
            }
            else
            {
                return new RequestResult<IEnumerable<Alignment.Translation.Translation>>(translations.OrderBy(t => t.SourceToken.TokenId.ToString()));
            }
        }
     }
}
