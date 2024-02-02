using MediatR;
using Microsoft.Extensions.Logging;
using SIL.Machine.Corpora;
using ClearBible.Engine.Proto;
using ClearDashboard.DAL.CQRS;
using ClearEngineWebApi.ProtoBufUtils;
using ClearEngineWebApi.Corpora;

namespace ClearDashboard.DAL.Alignment.Features.Corpora
{
    public class TokenizeTextCorpusCommandHandler : IRequestHandler<TokenizeTextCorpusCommand, RequestResult<ITextCorpus>>
	{
        private readonly ILogger _logger;
		private ClearEngineClientWebSocket _engineClient;

        public TokenizeTextCorpusCommandHandler(ILogger<TokenizeTextCorpusCommandHandler> logger, ClearEngineClientWebSocket engineClient)
        {
            _logger = logger;
			_engineClient = engineClient;
        }

		public async Task<RequestResult<ITextCorpus>> Handle(TokenizeTextCorpusCommand request, CancellationToken cancellationToken)
		{
			var tokenizedTextCorpus = await RemoteTokenize(request.TextCorpus, request.TokenizeTransformChain, cancellationToken);
			if (tokenizedTextCorpus == default)
			{
				_logger.LogInformation("Remote tokenization failed - switching to local");
				(tokenizedTextCorpus, _) = await TokenizedCorpusUtils.TokenizeTextCorpus(
					request.TextCorpus, 
					request.TokenizeTransformChain,
					_logger);
			}
			else
			{
				_logger.LogInformation("Remote tokenization succeeded");
			}

			return new RequestResult<ITextCorpus>(tokenizedTextCorpus);
		}

		private async Task<ITextCorpus?> RemoteTokenize(ITextCorpus textCorpus, IEnumerable<string> tokenizeTransformChain, CancellationToken cancellationToken)
		{
			try
			{
				await _engineClient.OpenWebSocket(cancellationToken);

				var textCorpusProto = CorporaConverter.ToProtocolBuf(textCorpus);

				var tokenizationRequest = new TokenizationRequest();
				tokenizationRequest.TokenizeTransformChainFullNames.AddRange(tokenizeTransformChain);
				tokenizationRequest.TextCorpus = textCorpusProto;

				var tokensTextCorpus = await _engineClient
					.SendReceiveBinary<TokenizationRequest, TokensTextCorpus>(tokenizationRequest, cancellationToken);

				// Eventually, we should perhaps leave the WebSocket open
				// during the running of Dashboard (and let shutdown/dispose
				// close the connection).
				await _engineClient.CloseWebSocket("Remote tokenization succeeded", cancellationToken);

				var (tokenizedTextCorpus, versification) = CorporaConverter.FromProtocolBuf(tokensTextCorpus);
				return tokenizedTextCorpus;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Exception attempting to remote tokenize");
				return default;
			}
		}
	}
}