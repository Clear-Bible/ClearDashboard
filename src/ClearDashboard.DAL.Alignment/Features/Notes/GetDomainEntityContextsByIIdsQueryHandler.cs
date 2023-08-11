using ClearBible.Engine.Corpora;
using ClearBible.Engine.Persistence;
using ClearBible.Engine.Utils;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Notes;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using ClearDashboard.DataAccessLayer.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SIL.Linq;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Web;
using static ClearBible.Engine.Persistence.FileGetBookIds;

//USE TO ACCESS Models
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.DAL.Alignment.Features.Notes
{
     public class GetDomainEntityContextsByIIdsQueryHandler : ProjectDbContextQueryHandler<GetDomainEntityContextsByIIdsQuery,
        RequestResult<Dictionary<IId, Dictionary<string, string>>>, Dictionary<IId, Dictionary<string, string>>>
    {
        public GetDomainEntityContextsByIIdsQueryHandler( 
            ProjectDbContextFactory? projectNameDbContextFactory, 
            IProjectProvider projectProvider, 
            ILogger<GetDomainEntityContextsByIIdsQueryHandler> logger) 
            : base(projectNameDbContextFactory, projectProvider, logger)
        {
        }

        protected override async Task<RequestResult<Dictionary<IId, Dictionary<string, string>>>> GetDataAsync(GetDomainEntityContextsByIIdsQuery request, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;

            var grouped = request.DomainEntityIIds
                .GroupBy(e => e.GetType().FindEntityIdGenericType()?.Name ?? $"__{e.GetType().Name}")
                .ToDictionary(
                    g => g.Key, 
                    g => g.Select(g => (IId: g, g.Id))
                );

            var notFound = grouped.Keys.Where(k => k.StartsWith("__")).Select(k => k[2..]);
            if (notFound.Any())
            {
                return new RequestResult<Dictionary<IId, Dictionary<string, string>>>
                (
                    success: false,
                    message: $"Unsupported domain entity id type(s) found:  {string.Join(", ", notFound)}"
                );
            }

            var bookNumbersToAbbreviations =
                FileGetBookIds.BookIds.ToDictionary(x => int.Parse(x.silCannonBookNum),
                    x => x.silCannonBookAbbrev);

            var domainEntityContexts = BuildDomainEntityContexts(
                grouped, 
                bookNumbersToAbbreviations, 
                cancellationToken);

            return new RequestResult<Dictionary<IId, Dictionary<string, string>>>(domainEntityContexts);
        }

        private Dictionary<IId, Dictionary<string, string>> BuildDomainEntityContexts(
            Dictionary<string, IEnumerable<(IId IId, Guid Id)>> grouped, 
            Dictionary<int, string> bookNumbersToAbbreviations,
            CancellationToken cancellationToken)
        {
            var domainEntityContexts = new Dictionary<IId, Dictionary<string, string>>();
            foreach (var kvp in grouped)
            {
                cancellationToken.ThrowIfCancellationRequested();

                switch (true)
                {
                    case true when kvp.Key == typeof(TokenId).Name:
                        var modelTypeNames = ProjectDbContext!.TokenComponents
                            .Where(e => kvp.Value.Select(ids => ids.Id).Contains(e.Id))
                            .Select(e => new { Id = e.Id, e.GetType().Name })
                            .ToList();

                        var tokenIds = modelTypeNames
                            .Where(e => e.Name == "Token")
                            .Select(e => e.Id);

                        if (tokenIds.Any())
                        {
                            ProjectDbContext!.Tokens
                                .Include(t => t.TokenizedCorpus)
                                    .ThenInclude(tc => tc!.Corpus)
                                .Where(e => kvp.Value.Select(ids => ids.Id).Contains(e.Id))
                                .ForEach(e =>
                                    domainEntityContexts.Add(
                                        new EntityId<TokenId>() { Id = e.Id },
                                        BuildTokenComponentContext(e, bookNumbersToAbbreviations))
                                );
                        }

                        var tokenComposites = modelTypeNames
                            .Where(e => e.Name == "TokenComposite")
                            .Select(e => e.Id);

                        if (tokenComposites.Any())
                        {
                            ProjectDbContext!.TokenComposites
                                .Include(t => t.Tokens)
                                .Include(t => t.TokenizedCorpus)
                                    .ThenInclude(tc => tc!.Corpus)
                                .Where(e => kvp.Value.Select(ids => ids.Id).Contains(e.Id))
                                .ForEach(e =>
                                    domainEntityContexts.Add(
                                        new EntityId<TokenId>() { Id = e.Id },
                                        BuildTokenComponentContext(e, bookNumbersToAbbreviations))
                                );
                        }
                        break;

                    case true when kvp.Key == typeof(CorpusId).Name:
                        ProjectDbContext!.Corpa
                            .Where(e => kvp.Value.Select(ids => ids.Id).Contains(e.Id))
                            .ForEach(e =>
                            {
                                var domainEntityContext = new Dictionary<string, string>
                                {
                                    { "Corpus.DisplayName", e.DisplayName ?? e.Name ?? string.Empty },
                                    { "URI", "CorpusId://" }
                                };

                                domainEntityContexts.Add(new EntityId<CorpusId>() { Id = e.Id }, domainEntityContext);
                            });
                        break;

                    case true when kvp.Key == typeof(TokenizedTextCorpusId).Name:
                        ProjectDbContext!.TokenizedCorpora
                            .Include(tc => tc.Corpus)
                            .Where(e => kvp.Value.Select(ids => ids.Id).Contains(e.Id))
                            .ForEach(e =>
                            {
                                var domainEntityContext = new Dictionary<string, string>
                                {
                                    { "Corpus.DisplayName", e.Corpus!.DisplayName ?? e.Corpus!.Name ?? string.Empty },
                                    { "TokenizedCorpus.DisplayName", e.DisplayName ?? string.Empty },
                                    { "URI", "TokenizedTextCorpusId://" }
                                };

                                domainEntityContexts.Add(new EntityId<TokenizedTextCorpusId>() { Id = e.Id }, domainEntityContext);
                            });
                        break;


                    case true when kvp.Key == typeof(ParallelCorpusId).Name:
                        ProjectDbContext!.ParallelCorpa
                            .Include(pc => pc.SourceTokenizedCorpus)
                                .ThenInclude(tc => tc!.Corpus)
                            .Include(pc => pc.TargetTokenizedCorpus)
                                .ThenInclude(tc => tc!.Corpus)
                            .Where(e => kvp.Value.Select(ids => ids.Id).Contains(e.Id))
                            .ForEach(e =>
                            {
                                var domainEntityContext = new Dictionary<string, string>
                                {
                                    { "SourceCorpus.DisplayName", e.SourceTokenizedCorpus!.Corpus!.DisplayName ?? e.SourceTokenizedCorpus!.Corpus!.Name ?? string.Empty },
                                    { "TargetCorpus.DisplayName", e.TargetTokenizedCorpus!.Corpus!.DisplayName ?? e.TargetTokenizedCorpus!.Corpus!.Name ?? string.Empty },
                                    { "SourceTokenizedCorpus.DisplayName", e.SourceTokenizedCorpus!.DisplayName ?? string.Empty },
                                    { "TargetTokenizedCorpus.DisplayName", e.TargetTokenizedCorpus!.DisplayName ?? string.Empty },
                                    { "URI", "ParallelCorpusId://" }
                                };

                                domainEntityContexts.Add(new EntityId<ParallelCorpusId>() { Id = e.Id }, domainEntityContext);
                            });
                        break;


                    case true when kvp.Key == typeof(AlignmentId).Name:
                        ProjectDbContext!.Alignments
                            .Include(a => a.SourceTokenComponent)
                            .Include(a => a.AlignmentSet)
                            .Where(e => kvp.Value.Select(ids => ids.Id).Contains(e.Id))
                            .ForEach(e =>
                            {
                                var domainEntityContext = new Dictionary<string, string>
                                {
                                    { "AlignmentSet.DisplayName", e.AlignmentSet!.DisplayName ?? string.Empty }
                                };

                                AddTokenComponentContext(e.SourceTokenComponent!, bookNumbersToAbbreviations, domainEntityContext);
                                domainEntityContext.Add("URI", "AlignmentId://");

                                domainEntityContexts.Add(new EntityId<AlignmentId>() { Id = e.Id }, domainEntityContext);
                            });
                        break;

                    case true when kvp.Key == typeof(AlignmentSetId).Name:
                        ProjectDbContext!.AlignmentSets
                            .Where(e => kvp.Value.Select(ids => ids.Id).Contains(e.Id))
                            .ForEach(e =>
                            {
                                var domainEntityContext = new Dictionary<string, string>
                                {
                                    { "AlignmentSet.DisplayName", e.DisplayName ?? string.Empty },
                                    { "URI", "AlignmentSetId://" }
                                };

                                domainEntityContexts.Add(new EntityId<AlignmentSetId>() { Id = e.Id }, domainEntityContext);
                            });
                        break;


                    case true when kvp.Key == typeof(TranslationId).Name:
                        ProjectDbContext!.Translations
                            .Include(a => a.SourceTokenComponent)
                            .Include(a => a.TranslationSet)
                            .Where(e => kvp.Value.Select(ids => ids.Id).Contains(e.Id))
                            .ForEach(e =>
                            {
                                var domainEntityContext = new Dictionary<string, string>
                                {
                                    { "TranslationSet.DisplayName", e.TranslationSet!.DisplayName ?? string.Empty }
                                };
                                AddTokenComponentContext(e.SourceTokenComponent!, bookNumbersToAbbreviations, domainEntityContext);
                                domainEntityContext.Add("URI", "TranslationId://");

                                domainEntityContexts.Add(new EntityId<TranslationId>() { Id = e.Id }, domainEntityContext);
                            });
                        break;

                    case true when kvp.Key == typeof(TranslationSetId).Name:
                        ProjectDbContext!.TranslationSets
                            .Where(e => kvp.Value.Select(ids => ids.Id).Contains(e.Id))
                            .ForEach(e =>
                            {
                                var domainEntityContext = new Dictionary<string, string>
                                {
                                    { "TranslationSet.DisplayName", e.DisplayName ?? string.Empty },
                                    { "URI", "TranslationSetId://" }
                                };

                                domainEntityContexts.Add(new EntityId<TranslationSetId>() { Id = e.Id }, domainEntityContext);
                            });
                        break;

                    case true when kvp.Key == typeof(NoteId).Name:
                        ProjectDbContext!.Notes
                            .Where(e => kvp.Value.Select(ids => ids.Id).Contains(e.Id))
                            .ForEach(e =>
                            {
                                var domainEntityContext = new Dictionary<string, string>
                                {
                                    { "Note.Text", e.Text ?? string.Empty },
                                    { "URI", "NoteId://" }
                                };

                                domainEntityContexts.Add(new EntityId<NoteId>() { Id = e.Id }, domainEntityContext);
                            });
                        break;

                    case true when kvp.Key == typeof(UserId).Name:
                        ProjectDbContext!.Users
                            .Where(e => kvp.Value.Select(ids => ids.Id).Contains(e.Id))
                            .ForEach(e =>
                            {
                                var domainEntityContext = new Dictionary<string, string>
                                {
                                    { "User.DisplayName", e.FullName ?? string.Empty },
                                    { "URI", "UserId://" }
                                };

                                domainEntityContexts.Add(new EntityId<UserId>() { Id = e.Id }, domainEntityContext);
                            });
                        break;

                    default:
                        throw new ArgumentException($"Unsupported domain entity id type found:  {kvp.Key}");
                }
            }

            return domainEntityContexts;
        }

        private Dictionary<string, string> BuildTokenComponentContext(
            Models.TokenComponent tokenComponent,
            Dictionary<int, string> bookNumbersToAbbreviations)
        {
            var domainEntityContext = new Dictionary<string, string>();

            domainEntityContext.Add("Corpus.DisplayName", tokenComponent.TokenizedCorpus!.Corpus!.DisplayName ?? tokenComponent.TokenizedCorpus!.Corpus!.Name ?? string.Empty);
            domainEntityContext.Add("TokenizedCorpus.DisplayName", tokenComponent.TokenizedCorpus!.DisplayName ?? tokenComponent.TokenizedCorpus!.Corpus!.Name ?? string.Empty);
            AddTokenComponentContext(tokenComponent, bookNumbersToAbbreviations, domainEntityContext);

            if (tokenComponent.GetType() == typeof(Models.Token))
            {
                domainEntityContext.Add("URI", "TokenId://");
            }
            else
            {
                domainEntityContext.Add("URI", "CompositeTokenId://");
            }

            return domainEntityContext;
        }

        private void AddTokenComponentContext(
            Models.TokenComponent tokenComponent,
            Dictionary<int, string> bookNumbersToAbbreviations,
            Dictionary<string, string> domainEntityContext)
        {
            string bookId = string.Empty;
            var tokenIdInfo = (tokenComponent.GetType() == typeof(Models.Token))
                ? ((Models.Token)tokenComponent)
                : ((Models.TokenComposite)tokenComponent).Tokens.FirstOrDefault();

            if (tokenIdInfo is not null)
            {
                if (bookNumbersToAbbreviations.ContainsKey(tokenIdInfo.BookNumber))
                {
                    bookId = bookNumbersToAbbreviations[tokenIdInfo.BookNumber];
                }
                else
                {
                    Logger.LogError($"Book number '{tokenIdInfo.BookNumber}' not found in FileGetBooks.BookIds");
                }
            }
            else
            {
                Logger.LogError($"TokenComposite {tokenComponent.Id} does not contain any tokens");
            }

            if (tokenComponent.GetType() == typeof(Models.Token))
            {
                Models.Token t = (Models.Token)tokenComponent;
                domainEntityContext.Add("TokenId.BookId", bookId);
                domainEntityContext.Add("TokenId.ChapterNumber", t.ChapterNumber.ToString());
                domainEntityContext.Add("TokenId.VerseNumber", t.VerseNumber.ToString());
                domainEntityContext.Add("TokenId.WordNumber", t.WordNumber.ToString());
                domainEntityContext.Add("TokenId.SubwordNumber", t.SubwordNumber.ToString());
            }
            else
            {
                Models.TokenComposite tc = (Models.TokenComposite)tokenComponent;
                domainEntityContext.Add("TokenId.BookId", bookId);
                domainEntityContext.Add("TokenId.ChapterNumber", tokenIdInfo?.ChapterNumber.ToString() ?? string.Empty);
                domainEntityContext.Add("TokenId.VerseNumber", tokenIdInfo?.VerseNumber.ToString() ?? string.Empty);
                domainEntityContext.Add("TokenId.WordNumber", tokenIdInfo?.WordNumber.ToString() ?? string.Empty);
                domainEntityContext.Add("TokenId.SubwordNumber", tokenIdInfo?.SubwordNumber.ToString() ?? string.Empty);
            }
        }
    }
}
