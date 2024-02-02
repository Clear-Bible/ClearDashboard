using MediatR;
using Microsoft.Extensions.Logging;
using SIL.Machine.Corpora;
using ClearBible.Engine.Proto;
using ClearDashboard.DAL.CQRS;
using ClearEngineWebApi.ProtoBufUtils;
using ClearBible.Engine.Corpora;
using ClearEngineWebApi.Corpora;
using SIL.Scripture;
using static ClearBible.Engine.Persistence.FileGetBookIds;

namespace ClearDashboard.DAL.Alignment.Features.Translation
{
    public class AlignTokenizedCorporaCommandHandler : IRequestHandler<AlignTokenizedCorporaCommand, RequestResult<(
			IEnumerable<AlignedTokenPairs> AlignTokenPairs,
			Dictionary<string, Dictionary<string, double>> TranslationModel
		)>>
	{
        private readonly ILogger _logger;
		private ClearEngineClientWebSocket _engineClient;

		public AlignTokenizedCorporaCommandHandler(ILogger<AlignTokenizedCorporaCommandHandler> logger, ClearEngineClientWebSocket engineClient)
        {
            _logger = logger;
			_engineClient = engineClient;
        }

		public async Task<RequestResult<(IEnumerable<AlignedTokenPairs> AlignTokenPairs, Dictionary<string, Dictionary<string, double>> TranslationModel)>> Handle(
			AlignTokenizedCorporaCommand request, 
			CancellationToken cancellationToken)
		{
			var alignmentTranslationModel = await RemoteAlignTokenizedCorpora(request, cancellationToken);
			if (alignmentTranslationModel == default)
			{
				_logger.LogInformation("Remote alignment failed - switching to local");
				alignmentTranslationModel = await LocalAlignTokenizedCorpora(request, cancellationToken);
			}
			else
			{
				_logger.LogInformation("Remote alignment succeeded");
			}

			return new RequestResult<(IEnumerable<AlignedTokenPairs> AlignTokenPairs, Dictionary<string, Dictionary<string, double>> TranslationModel)>(alignmentTranslationModel);
		}

		private async Task<(IEnumerable<AlignedTokenPairs> AlignTokenPairs,
						    Dictionary<string, Dictionary<string, double>> TranslationModel)>
			LocalAlignTokenizedCorpora(AlignTokenizedCorporaCommand request, CancellationToken cancellationToken)
		{
			var sourceTokenizedCorpusVers = await ExtractSourceTokenizedCorpus(request);
			var targetTokenizedCorpusVers = await ExtractTargetTokenizedCorpus(request);
			var verseMappings = request.VerseMappings;

			var (alignedTokenPairs, translationModel) = await ParallelCorpusUtils.RunAlignment(
				sourceTokenizedCorpusVers,
				targetTokenizedCorpusVers,
				request.IsTrainedSymmetrizedModel,
				TokenizedCorpusUtils.GetSmtModelTypeProto(request.SmtModelType),
				verseMappings,
				_logger,
				cancellationToken
			);

			return (alignedTokenPairs, translationModel);
		}

		private static async Task<(ITextCorpus TextCorpus, ScrVers? Versification)> ExtractSourceTokenizedCorpus(AlignTokenizedCorporaCommand request)
		{
			if (request.SourceTokenizedTextCorpus is not null)
			{
				return (request.SourceTokenizedTextCorpus, request.SourceTokenizedTextCorpus.GetVersificationOrDefault());
			}
			else if (request.SourceManuscriptCorpusType is not null)
			{
				return await TokenizedCorpusUtils.GetManuscriptCorpusAsync((LanguageCodeEnum)request.SourceManuscriptCorpusType);
			}
			else
			{
				throw new ArgumentNullException($"Oneof AlignTokenizedCorporaCommand SourceTokenizedTextCorpus or SourceManuscriptCorpusType must be supplied.");
			}
		}

		private static async Task<(ITextCorpus TextCorpus, ScrVers? Versification)> ExtractTargetTokenizedCorpus(AlignTokenizedCorporaCommand request)
		{
			if (request.TargetTokenizedTextCorpus is not null)
			{
				return (request.TargetTokenizedTextCorpus, request.TargetTokenizedTextCorpus.GetVersificationOrDefault());
			}
			else if (request.TargetManuscriptCorpusType is not null)
			{
				return await TokenizedCorpusUtils.GetManuscriptCorpusAsync((LanguageCodeEnum)request.TargetManuscriptCorpusType);
			}
			else
			{
				throw new ArgumentNullException($"Oneof AlignTokenizedCorporaCommand TargetTokenizedTextCorpus or TargetManuscriptCorpusType must be supplied.");
			}
		}

		private async Task<(
			IEnumerable<AlignedTokenPairs> AlignTokenPairs,
			Dictionary<string, Dictionary<string, double>> TranslationModel
		)> RemoteAlignTokenizedCorpora(AlignTokenizedCorporaCommand request, CancellationToken cancellationToken)
		{
			try
			{
				await _engineClient.OpenWebSocket(cancellationToken);

				var alignmentRequest = ToProtocolBuf(request);

				var tokensAlignment = await _engineClient
					.SendReceiveBinary<AlignmentRequest, TokensAlignment>(alignmentRequest, cancellationToken);

				// Eventually, we should perhaps leave the WebSocket open
				// during the running of Dashboard (and let shutdown/dispose
				// close the connection).
				await _engineClient.CloseWebSocket("Remote alignment succeeded", cancellationToken);

				var alignment = AlignmentConverter.FromProtocolBuf(tokensAlignment);
				return alignment;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Exception attempting to remote align");
				return default;
			}
		}

		private static AlignmentRequest ToProtocolBuf(AlignTokenizedCorporaCommand request)
		{
			var alignmentRequest = new AlignmentRequest
			{
				IsTrainedSymmetrizedModel = request.IsTrainedSymmetrizedModel,
				SmtModelType = TokenizedCorpusUtils.GetSmtModelTypeProto(request.SmtModelType)
			};

			if (request.SourceTokenizedTextCorpus is not null)
			{
				alignmentRequest.SourceTokenizedCorpus = CorporaConverter.ToProtocolBuf((
					request.SourceTokenizedTextCorpus,
					request.SourceTokenizedTextCorpus.GetVersificationOrDefault()));
			}
			else
			{
				alignmentRequest.SourceManuscriptCorpusType = request.SourceManuscriptCorpusType switch
				{
					LanguageCodeEnum.H => ManuscriptCorpusType.Hebrew,
					LanguageCodeEnum.G => ManuscriptCorpusType.Greek,
					_ => throw new NotSupportedException($"Unsupported LanguageCodeEnum: {request.SourceManuscriptCorpusType}"),
				};
			}

			if (request.TargetTokenizedTextCorpus is not null)
			{
				alignmentRequest.TargetTokenizedCorpus = CorporaConverter.ToProtocolBuf((
					request.TargetTokenizedTextCorpus,
					request.TargetTokenizedTextCorpus.GetVersificationOrDefault()));
			}
			else
			{
				alignmentRequest.TargetManuscriptCorpusType = request.TargetManuscriptCorpusType switch
				{
					LanguageCodeEnum.H => ManuscriptCorpusType.Hebrew,
					LanguageCodeEnum.G => ManuscriptCorpusType.Greek,
					_ => throw new NotSupportedException($"Unsupported LanguageCodeEnum: {request.TargetManuscriptCorpusType}"),
				};
			}

			if (request.VerseMappings is not null)
			{
				AlignmentConverter.AddVerseMappings(request.VerseMappings, alignmentRequest);
			}

			return alignmentRequest;
		}
	}
}