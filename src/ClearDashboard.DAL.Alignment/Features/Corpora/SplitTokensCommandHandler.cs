using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Features.Events;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

//USE TO ACCESS Models
using Models = ClearDashboard.DataAccessLayer.Models;
using Metadatum = ClearDashboard.DataAccessLayer.Models.Metadatum;
using ClearDashboard.DAL.Alignment.Features.Common;

namespace ClearDashboard.DAL.Alignment.Features.Corpora
{
    public class SplitTokensCommandHandler : ProjectDbContextCommandHandler<SplitTokensCommand,
        RequestResult<(IDictionary<TokenId, IEnumerable<CompositeToken>>, IDictionary<TokenId, IEnumerable<Token>>)>, 
        (IDictionary<TokenId, IEnumerable<CompositeToken>>, IDictionary<TokenId, IEnumerable<Token>>)>
    {
		private readonly IUserProvider _userProvider;
		private readonly IMediator _mediator;

        public SplitTokensCommandHandler(
            IMediator mediator,
            ProjectDbContextFactory? projectNameDbContextFactory, IProjectProvider projectProvider, IUserProvider userProvider,

			ILogger<SplitTokensCommandHandler> logger) : base(projectNameDbContextFactory, projectProvider,
            logger)
        {
            _userProvider = userProvider;
            _mediator = mediator;
        }

        protected override async Task<RequestResult<(IDictionary<TokenId, IEnumerable<CompositeToken>>, IDictionary<TokenId, IEnumerable<Token>>)>> SaveDataAsync(
            SplitTokensCommand request,
            CancellationToken cancellationToken)
        {
            if (request.TrainingText3 is null && request.SurfaceTextIndex > 0)
            {
                return new RequestResult<(IDictionary<TokenId, IEnumerable<CompositeToken>>, IDictionary<TokenId, IEnumerable<Token>>)>
                (
                    success: false,
                    message: $"Request '{nameof(request.TrainingText3)}' can only be null if '{nameof(request.SurfaceTextIndex)}' == 0 (incoming value: [{request.SurfaceTextIndex}])"
                );
            }

            if (!request.TokenIdsWithSameSurfaceText.Any())
            {
                return new RequestResult<(IDictionary<TokenId, IEnumerable<CompositeToken>>, IDictionary<TokenId, IEnumerable<Token>>)>
                (
                    success: false,
                    message: $"Request '{nameof(request.TokenIdsWithSameSurfaceText)}' was empty"
                );
            }

            var requestTokenIdGuids = request.TokenIdsWithSameSurfaceText.Select(e => e.Id);

            var tokensDbQueryable = ProjectDbContext.Tokens
                .Include(t => t.TokenCompositeTokenAssociations)
                .Include(t => t.TokenizedCorpus)
                    .ThenInclude(tc => tc!.SourceParallelCorpora)
                .Include(t => t.TokenizedCorpus)
                    .ThenInclude(tc => tc!.TargetParallelCorpora)
                .Include(t => t.Translations.Where(a => a.Deleted == null))
                .Include(t => t.SourceAlignments.Where(a => a.Deleted == null))
                .Include(t => t.TargetAlignments.Where(a => a.Deleted == null))
                .Include(t => t.TokenVerseAssociations.Where(a => a.Deleted == null))
                .Where(t => t.TokenizedCorpusId == request.TokenizedTextCorpusId.Id);

            var tokensDb = await tokensDbQueryable
                .Where(t => requestTokenIdGuids.Contains(t.Id))
                .ToListAsync(cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            var noteAssociationsDb = ProjectDbContext.NoteDomainEntityAssociations
                .Where(dea => dea.DomainEntityIdGuid != null)
                .Where(dea => requestTokenIdGuids.Contains((Guid)dea.DomainEntityIdGuid!))
                .ToList();

            var missingTokenIdGuids = requestTokenIdGuids.Except(tokensDb.Select(e => e.Id));
            if (missingTokenIdGuids.Any())
            {
                return new RequestResult<(IDictionary<TokenId, IEnumerable<CompositeToken>>, IDictionary<TokenId, IEnumerable<Token>>)>
                (
                    success: false,
                    message: $"Request TokenId(s) '{string.Join(",", missingTokenIdGuids)}' not found in database as part of TokenizedCorpusId '{request.TokenizedTextCorpusId.Id}'"
                );
            }

            var surfaceText = tokensDb.First().SurfaceText;
            if (string.IsNullOrEmpty(surfaceText))
            {
                return new RequestResult<(IDictionary<TokenId, IEnumerable<CompositeToken>>, IDictionary<TokenId, IEnumerable<Token>>)>
                (
                    success: false,
                    message: $"First matching Token in database has SurfaceText that is null or empty (TokenId: '{tokensDb.First().Id}')"
                );
            }

            if (!tokensDb.Select(e => e.SurfaceText).All(e => e == surfaceText))
            {
                return new RequestResult<(IDictionary<TokenId, IEnumerable<CompositeToken>>, IDictionary<TokenId, IEnumerable<Token>>)>
                (
                    success: false,
                    message: $"Tokens in database matching request TokenIds do not all having SurfaceText matching: '{surfaceText}'"
                );
            }

            if (tokensDb.Count == 1 && request.PropagateTo != SplitTokenPropagationScope.None)
            {
                var tokenToPropagate = tokensDb.First();

                var tokensDbPropagateQueryable = tokensDbQueryable
                    .Where(t => t.SurfaceText == tokenToPropagate.SurfaceText)
                    .Where(t => t.Id != tokenToPropagate.Id);

                if (request.PropagateTo == SplitTokenPropagationScope.Book)
                {
                    tokensDbPropagateQueryable = tokensDbPropagateQueryable
                        .Where(t => t.BookNumber == tokenToPropagate.BookNumber);
                }

                if (request.PropagateTo == SplitTokenPropagationScope.BookChapter)
                {
                    tokensDbPropagateQueryable = tokensDbPropagateQueryable
                        .Where(t => t.BookNumber == tokenToPropagate.BookNumber)
                        .Where(t => t.ChapterNumber == tokenToPropagate.ChapterNumber);
                }

                if (request.PropagateTo == SplitTokenPropagationScope.BookChapterVerse)
                {
                    tokensDbPropagateQueryable = tokensDbPropagateQueryable
                        .Where(t => t.BookNumber == tokenToPropagate.BookNumber)
                        .Where(t => t.ChapterNumber == tokenToPropagate.ChapterNumber)
                        .Where(t => t.VerseNumber == tokenToPropagate.VerseNumber);
                }

                tokensDb.AddRange(await tokensDbPropagateQueryable.ToListAsync(cancellationToken));
            }

            if (request.SurfaceTextIndex < 0 ||
                request.SurfaceTextLength <= 0 ||
                (request.SurfaceTextIndex + request.SurfaceTextLength) >= surfaceText.Length)
            {
                return new RequestResult<(IDictionary<TokenId, IEnumerable<CompositeToken>>, IDictionary<TokenId, IEnumerable<Token>>)>
                (
                    success: false,
                    message: $"Request SurfaceTextIndex [{request.SurfaceTextIndex}] and/or SurfaceTextLength [{request.SurfaceTextLength}] is out of range for SurfaceText '{surfaceText}'"
                );
            }

			using var splitTokenDbCommands = await SplitTokenDbCommands.CreateAsync(ProjectDbContext, _userProvider, Logger, cancellationToken);

			var idx = 0; 
            var replacementTokenInfos = new (
				string surfaceText,
				string trainingText,
				string? tokenType,
                string? type, 
                Guid? grammarId)[(request.SurfaceTextIndex > 0) ? 3 : 2];

            if (request.SurfaceTextIndex > 0)
            {
				replacementTokenInfos[idx++] = (
					surfaceText: surfaceText[0..request.SurfaceTextIndex],
					trainingText: request.TrainingText1,
                    default(string?),
                    default(string?),
                    default(Guid?));
			}
			replacementTokenInfos[idx++] = (
				surfaceText: surfaceText[request.SurfaceTextIndex..(request.SurfaceTextIndex + request.SurfaceTextLength)],
				trainingText: (request.SurfaceTextIndex > 0) ? request.TrainingText2 : request.TrainingText1,
                default(string?),
                default(string?),
                default(Guid?));
			replacementTokenInfos[idx++] = (
				surfaceText: surfaceText[(request.SurfaceTextIndex + request.SurfaceTextLength)..^0],
				trainingText: (request.SurfaceTextIndex > 0) ? request.TrainingText3! : request.TrainingText2,
                default(string?),
                default(string?),
                default(Guid?));

			var (
					splitCompositeTokensByIncomingTokenId,
					splitChildTokensByIncomingTokenId
				) = SplitTokenUtil.SplitTokensIntoReplacements(
				tokensDb,
				replacementTokenInfos,
				request.CreateParallelComposite,
				splitTokenDbCommands,
				cancellationToken
			);

			await splitTokenDbCommands.ExecuteBulkOperationsAsync(cancellationToken);
			await splitTokenDbCommands.CommitTransactionAsync(_mediator, cancellationToken);

			return new RequestResult<(IDictionary<TokenId, IEnumerable<CompositeToken>>, IDictionary<TokenId, IEnumerable<Token>>)>
			(
				(splitCompositeTokensByIncomingTokenId, splitChildTokensByIncomingTokenId)
			);
        }
    }
}