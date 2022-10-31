using ClearBible.Engine.Corpora;
using ClearBible.Engine.Utils;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Notes;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SIL.Machine.FiniteState;

//USE TO ACCESS Models
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.DAL.Alignment.Features.Notes
{
     public class GetFullDomainEntityIdsByIIdsQueryHandler : ProjectDbContextQueryHandler<GetFullDomainEntityIdsByIIdsQuery,
        RequestResult<IEnumerable<IId>>, IEnumerable<IId>>
    {
        public GetFullDomainEntityIdsByIIdsQueryHandler( 
            ProjectDbContextFactory? projectNameDbContextFactory, 
            IProjectProvider projectProvider, 
            ILogger<GetFullDomainEntityIdsByIIdsQueryHandler> logger) 
            : base(projectNameDbContextFactory, projectProvider, logger)
        {
        }

        protected override async Task<RequestResult<IEnumerable<IId>>> GetDataAsync(GetFullDomainEntityIdsByIIdsQuery request, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;

            var grouped = request.DomainEntityIIds
                .GroupBy(e => e.GetType().FindEntityIdGenericType()?.Name ?? $"__{e.GetType().Name}")
                .ToDictionary(g => g.Key, g => g.Select(g => g.Id));

            var notFound = grouped.Keys.Where(k => k.StartsWith("__")).Select(k => k[2..]);
            if (notFound.Any())
            {
                return new RequestResult<IEnumerable<IId>>
                (
                    success: false,
                    message: $"Unsupported domain entity id type(s) found:  {string.Join(", ", notFound)}"
                );
            }

            var fullIds = new List<IId>();
            foreach (var kvp in grouped)
            {
                switch (true)
                {
                    case true when kvp.Key == typeof(TokenId).Name:
                        fullIds.AddRange(ProjectDbContext!.Tokens
                            .Where(e => kvp.Value.Contains(e.Id))
                            .Select(e => ModelHelper.BuildTokenId(e)));
                        break;

                    case true when kvp.Key == typeof(CompositeTokenId).Name:
                        fullIds.AddRange(ProjectDbContext!.TokenComposites
                            .Include(e => e.Tokens)
                            .Where(e => kvp.Value.Contains(e.Id))
                            .Select(e => ModelHelper.BuildTokenId(e)));
                        break;

                    case true when kvp.Key == typeof(CorpusId).Name:
                        fullIds.AddRange(ProjectDbContext!.Corpa
                            .Where(e => kvp.Value.Contains(e.Id))
                            .Select(e => ModelHelper.BuildCorpusId(e)));
                        break;

                    case true when kvp.Key == typeof(TokenizedTextCorpusId).Name:
                        fullIds.AddRange(
                            ModelHelper.AddIdIncludesTokenizedCorpaQuery(ProjectDbContext!)
                                .Where(e => kvp.Value.Contains(e.Id))
                                .Select(e => ModelHelper.BuildTokenizedTextCorpusId(e)));
                        break;

                    case true when kvp.Key == typeof(ParallelCorpusId).Name:
                        fullIds.AddRange(
                            ModelHelper.AddIdIncludesParallelCorpaQuery(ProjectDbContext!)
                                .Where(e => kvp.Value.Contains(e.Id))
                                .Select(e => ModelHelper.BuildParallelCorpusId(e)));
                        break;

                    case true when kvp.Key == typeof(AlignmentId).Name:
                        fullIds.AddRange(
                            ModelHelper.AddIdIncludesAlignmentsQuery(ProjectDbContext!)
                                .Where(e => kvp.Value.Contains(e.Id))
                                .Select(e => ModelHelper.BuildAlignmentId(e)));
                        break;

                    case true when kvp.Key == typeof(AlignmentSetId).Name:
                        fullIds.AddRange(
                            ModelHelper.AddIdIncludesAlignmentSetsQuery(ProjectDbContext!)
                                .Where(e => kvp.Value.Contains(e.Id))
                                .Select(e => ModelHelper.BuildAlignmentSetId(e)));
                        break;

                    case true when kvp.Key == typeof(TranslationId).Name:
                        fullIds.AddRange(
                            ModelHelper.AddIdIncludesTranslationsQuery(ProjectDbContext!)
                                .Where(e => kvp.Value.Contains(e.Id))
                                .Select(e => ModelHelper.BuildTranslationId(e)));
                        break;

                    case true when kvp.Key == typeof(TranslationSetId).Name:
                        fullIds.AddRange(
                            ModelHelper.AddIdIncludesTranslationSetsQuery(ProjectDbContext!)
                                .Where(e => kvp.Value.Contains(e.Id))
                                .Select(e => ModelHelper.BuildTranslationSetId(e)));
                        break;

                    case true when kvp.Key == typeof(NoteId).Name:
                        fullIds.AddRange(
                            ModelHelper.AddIdIncludesNotesQuery(ProjectDbContext!)
                                .Where(e => kvp.Value.Contains(e.Id))
                                .Select(e => ModelHelper.BuildNoteId(e)));
                        break;

                    case true when kvp.Key == typeof(UserId).Name:
                        fullIds.AddRange(
                            ProjectDbContext!.Users
                                .Where(e => kvp.Value.Contains(e.Id))
                                .Select(e => ModelHelper.BuildUserId(e)));
                        break;
                    default:
                        return new RequestResult<IEnumerable<IId>>
                        (
                            success: false,
                            message: $"Unsupported domain entity id type found:  {kvp.Key}"
                        );
                }
            }

            return new RequestResult<IEnumerable<IId>>(fullIds);
        }
    }
}
