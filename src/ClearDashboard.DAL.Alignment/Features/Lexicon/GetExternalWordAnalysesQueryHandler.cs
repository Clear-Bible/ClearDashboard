using Autofac.Features.AttributeFilters;
using ClearDashboard.DAL.Alignment.Lexicon;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Annotations;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.DAL.Alignment.Features.Lexicon
{
    public class GetExternalWordAnalysesQueryHandler : IRequestHandler<GetExternalWordAnalysesQuery, RequestResult<IEnumerable<Alignment.Lexicon.WordAnalysis>>>
    {
        private readonly ILogger<GetExternalWordAnalysesQueryHandler> _logger;
        private readonly IMediator _mediator;
        private readonly IUserProvider _userProvider;

        private Func<string?, IRequest<RequestResult<IEnumerable<DataAccessLayer.Models.Lexicon_WordAnalysis>>>> _externalWordAnalysesQueryFactory { get; }

        public GetExternalWordAnalysesQueryHandler(
            [NotNull] ILogger<GetExternalWordAnalysesQueryHandler> logger,
            [NotNull] IMediator mediator,
            [NotNull] IUserProvider userProvider,
            [KeyFilter("External")] Func<string?, IRequest<RequestResult<IEnumerable<DataAccessLayer.Models.Lexicon_WordAnalysis>>>> externalWordAnalysesQueryFactory)
        {
            _logger = logger;
            _mediator = mediator;
            _userProvider = userProvider;
			_externalWordAnalysesQueryFactory = externalWordAnalysesQueryFactory;
        }

        public async Task<RequestResult<IEnumerable<Alignment.Lexicon.WordAnalysis>>> Handle(GetExternalWordAnalysesQuery request, CancellationToken cancellationToken)
        {
            var externalWordAnalysesQuery = _externalWordAnalysesQueryFactory(request.ProjectId);
            var result = await _mediator.Send(externalWordAnalysesQuery, cancellationToken);
            result.ThrowIfCanceledOrFailed();

            foreach (var wordAnalysis in result.Data!)
            {
                wordAnalysis.User = _userProvider.CurrentUser!;
                wordAnalysis.UserId = _userProvider.CurrentUser!.Id;

                foreach (var lexeme in wordAnalysis.Lexemes)
                {
                    lexeme.User = _userProvider.CurrentUser!;
                    lexeme.UserId = _userProvider.CurrentUser!.Id;
                }
            }

            var wordAnalyses = 
                result.Data!
                    .Select(e => ModelHelper.BuildWordAnalysis(e, true))
                    .ToList()
                ;

            return new RequestResult<IEnumerable<Alignment.Lexicon.WordAnalysis>>(wordAnalyses);
        }
    }
}
