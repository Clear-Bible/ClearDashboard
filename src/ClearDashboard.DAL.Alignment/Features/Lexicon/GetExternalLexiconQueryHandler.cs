using Autofac.Features.AttributeFilters;
using ClearDashboard.DAL.Alignment.Lexicon;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Annotations;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.DAL.Alignment.Features.Lexicon
{
    public class GetExternalLexiconQueryHandler : IRequestHandler<GetExternalLexiconQuery, RequestResult<Alignment.Lexicon.Lexicon>>
    {
        private readonly ILogger<GetExternalLexiconQueryHandler> _logger;
        private readonly IMediator _mediator;
        private readonly IUserProvider _userProvider;

        private IRequest<RequestResult<DataAccessLayer.Models.Lexicon_Lexicon>> _externalLexiconQuery { get; }

        public GetExternalLexiconQueryHandler(
            [NotNull] ILogger<GetExternalLexiconQueryHandler> logger,
            [NotNull] IMediator mediator,
            [NotNull] IUserProvider userProvider,
            [KeyFilter("External")] IRequest<RequestResult<DataAccessLayer.Models.Lexicon_Lexicon>> externalLexiconQuery)
        {
            _logger = logger;
            _mediator = mediator;
            _userProvider = userProvider;
            _externalLexiconQuery = externalLexiconQuery;
        }

        public async Task<RequestResult<Alignment.Lexicon.Lexicon>> Handle(GetExternalLexiconQuery request, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(_externalLexiconQuery, cancellationToken);
            result.ThrowIfCanceledOrFailed();

            var lexemes = new List<Alignment.Lexicon.Lexeme>();
            foreach (var lexeme in result.Data!.Lexemes)
            {
                lexeme.User = _userProvider.CurrentUser!;
                lexeme.UserId = _userProvider.CurrentUser!.Id;

                foreach (var meaning in lexeme.Meanings)
                {
                    meaning.User = _userProvider.CurrentUser!;
                    meaning.UserId = _userProvider.CurrentUser!.Id;

                    foreach (var translation in meaning.Translations)
                    {
                        translation.User = _userProvider.CurrentUser!;
                        translation.UserId = _userProvider.CurrentUser!.Id;
                    }

                    foreach (var semanticDomain in meaning.SemanticDomains)
                    {
                        semanticDomain.User = _userProvider.CurrentUser!;
                        semanticDomain.UserId = _userProvider.CurrentUser!.Id;
                    }
                }
            }

            var lexicon = new Alignment.Lexicon.Lexicon(
                result.Data!.Lexemes
                    .Select(e => ModelHelper.BuildLexeme(e, null, true))
                    .ToList()
                );

            return new RequestResult<Alignment.Lexicon.Lexicon>(lexicon);
        }
    }
}
