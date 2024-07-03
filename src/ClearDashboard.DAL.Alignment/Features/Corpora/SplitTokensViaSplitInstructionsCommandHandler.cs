using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Features.Common;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using ClearDashboard.DataAccessLayer.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Token = ClearBible.Engine.Corpora.Token;

namespace ClearDashboard.DAL.Alignment.Features.Corpora;

public record GetTokenIdsWithCommonSurfaceTextQuery(string SurfaceText): ProjectRequestQuery<IDictionary<TokenId, List<Guid>>>;

public class GetTokenIdsWithCommonSurfaceTextQueryHandler : ProjectDbContextQueryHandler<
    GetTokenIdsWithCommonSurfaceTextQuery, RequestResult<IDictionary<TokenId, List<Guid>>>, IDictionary<TokenId, List<Guid>>>
{
    public GetTokenIdsWithCommonSurfaceTextQueryHandler(ProjectDbContextFactory? projectNameDbContextFactory, IProjectProvider? projectProvider, ILogger logger) : base(projectNameDbContextFactory, projectProvider, logger)
    {
    }

    protected override async Task<RequestResult<IDictionary<TokenId, List<Guid>>>> GetDataAsync(GetTokenIdsWithCommonSurfaceTextQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var tokenIdsWithCommonSurfaceText = ProjectDbContext!.Tokens
                .Include(e => e.TokenCompositeTokenAssociations)
                .Where(e => e.SurfaceText == request.SurfaceText)
                .ToDictionary(ModelHelper.BuildTokenId, e => e.TokenCompositeTokenAssociations.Select(ta => ta.TokenCompositeId).ToList());

            await Task.CompletedTask;

            return new RequestResult<IDictionary<TokenId, List<Guid>>>(tokenIdsWithCommonSurfaceText);
        }
        catch (Exception e)
        {
            return new RequestResult<IDictionary<TokenId, List<Guid>>>(success: false, message: e.Message);
        }
       
    }
}

public class SplitTokensViaSplitInstructionsCommandHandler : ProjectDbContextCommandHandler<SplitTokensViaSplitInstructionsCommand,
    RequestResult<(IDictionary<TokenId, IEnumerable<CompositeToken>>, IDictionary<TokenId, IEnumerable<Token>>)>,
    (IDictionary<TokenId, IEnumerable<CompositeToken>>, IDictionary<TokenId, IEnumerable<Token>>)>
{
	private readonly IUserProvider _userProvider;
	private readonly IMediator _mediator;

    public SplitTokensViaSplitInstructionsCommandHandler(
        IMediator mediator,
        ProjectDbContextFactory? projectNameDbContextFactory, 
		IProjectProvider projectProvider,
		IUserProvider userProvider,
		ILogger<SplitTokensCommandHandler> logger) : base(projectNameDbContextFactory, projectProvider,
        logger)
    {
		_userProvider = userProvider;
		_mediator = mediator;
    }

    protected override async Task<RequestResult<(IDictionary<TokenId, IEnumerable<CompositeToken>>, IDictionary<TokenId, IEnumerable<Token>>)>> SaveDataAsync(
        SplitTokensViaSplitInstructionsCommand request,
        CancellationToken cancellationToken)
    {
        
        if (!request.SplitInstructions.Validate())
        {
            return new RequestResult<(IDictionary<TokenId, IEnumerable<CompositeToken>>, IDictionary<TokenId, IEnumerable<Token>>)>
                (
                    success: false,
                    message: request.SplitInstructions.ErrorMessage
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

		using var splitTokenDbCommands = await SplitTokenDbCommands.CreateAsync(ProjectDbContext, _userProvider, Logger, cancellationToken);

		var replacementTokenInfos = new (
			string surfaceText,
			string trainingText,
			string? tokenType,
            string? circumfixGroup,
            Guid? grammarId)[request.SplitInstructions.Count];

		for (var i = 0; i < request.SplitInstructions.Count; i++)
		{
			var splitInstruction = request.SplitInstructions[i];
			replacementTokenInfos[i] = (
				surfaceText: splitInstruction.TokenText,
				trainingText: splitInstruction.TrainingText, 
				tokenType: splitInstruction.TokenType,
                circumfixGroup: splitInstruction.CircumfixGroup,
                grammarId: splitInstruction.GrammarId

            )!;
		}

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



