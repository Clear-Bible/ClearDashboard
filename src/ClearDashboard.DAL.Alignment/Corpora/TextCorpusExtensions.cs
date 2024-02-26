using Autofac;
using ClearBible.Engine.Corpora;
using EC = ClearBible.Engine.EngineCommands;
using ClearBible.Engine.Exceptions;
using ClearDashboard.DAL.Alignment.Exceptions;
using ClearDashboard.DAL.Alignment.Features;
using ClearDashboard.DAL.Alignment.Features.Corpora;
using MediatR;
using SIL.Machine.Corpora;
using SIL.Scripture;

namespace ClearDashboard.DAL.Alignment.Corpora
{
	public static class TextCorpusExtensions
	{
		/// <summary>
		/// Creates a new Corpus from a non-TokenizedTextCorpus, associated CorpusVersion,  a new associated TokenizedCorpus, and all the tokens within the corpus. 
		/// </summary>
		/// <param name="textCorpus">textCorpus must already be tokenized and not of type TokenizedTextCorpus. This is done in this fashion:
		///     textCorpus
		///         .Tokenize<LatinWordTokenizer>()
		///         .Transform<IntoTokensTextRowProcessor>()
		/// </param>
		/// <param name="textCorpus"></param>
		/// <param name="mediator"></param>
		/// <param name="corpusId"></param>
		/// <param name="displayName"></param>
		/// <param name="tokenizationFunction"></param>
		/// <param name="token"></param>
		/// <returns></returns>
		/// <exception cref="InvalidTypeEngineException"></exception>
		/// <exception cref="MediatorErrorEngineException"></exception>
		public static async Task<TokenizedTextCorpus> Create(this ITextCorpus textCorpus, IMediator mediator, CorpusId corpusId, string displayName, string tokenizationFunction, ScrVers? versification = null, CancellationToken token = default, bool useCache = false)
		{
			try
			{
				_ = textCorpus.Cast<TokensTextRow>();
			}
			catch (InvalidCastException)
			{
				throw new InvalidTypeEngineException(message: $"Corpus must be tokenized and transformed into TokensTextRows, e.g. corpus.Tokenize<LatinWordTokenizer>().Transform<IntoTokensTextRowProcessor>()");
			}

			var command = new CreateTokenizedCorpusFromTextCorpusCommand(textCorpus, corpusId, displayName, tokenizationFunction, versification ?? ScrVers.Original);

			var result = await mediator.Send(command, token);
			result.ThrowIfCanceledOrFailed(true);

			var createdTokenizedTextCorpus = result.Data!;
			createdTokenizedTextCorpus.UseCache = useCache;
			return createdTokenizedTextCorpus;
		}

		public static async Task<ITextCorpus> TokenizeTransformAsync(this TextCorpusTokenizerTransformer tokenizerTransformer, IComponentContext context, CancellationToken cancellationToken)
		{
			var tcReceiver = context.Resolve<EC.IEngineCommandReceiver<EC.TokenizeTextCorpusCommand,EC.TokensTextCorpus>>();
			var tcReply = await tcReceiver.RequestAsync(new EC.TokenizeTextCorpusCommand
			{
				TokenizeTransformChainFullNames = tokenizerTransformer.TokenizeTransformChain,
				TextCorpus = tokenizerTransformer.TextCorpus
			},
			cancellationToken);

			return tcReply.TokensTextRowCorpus;
		}
	}
}
