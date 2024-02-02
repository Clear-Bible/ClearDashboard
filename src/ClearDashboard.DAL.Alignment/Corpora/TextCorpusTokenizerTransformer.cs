using ClearDashboard.DAL.Alignment.Features;
using ClearDashboard.DAL.Alignment.Features.Corpora;
using MediatR;
using SIL.Machine.Corpora;
using SIL.Machine.Tokenization;

namespace ClearDashboard.DAL.Alignment.Corpora
{
	public class TextCorpusTokenizerTransformer
	{
		public TextCorpusTokenizerTransformer(ScriptureTextCorpus textCorpus)
		{
			TextCorpus = textCorpus;
			TokenizeTransformChain = new();
		}

		public TextCorpusTokenizerTransformer AddTokenizer<T>()
			where T : ITokenizer<string, int, string>
		{
			return AddTokenizer(typeof(T).FullName!);
		}

		public TextCorpusTokenizerTransformer AddTokenizer(string tokenizerClassName)
		{
			TokenizeTransformChain.Add($"Tokenize({tokenizerClassName})");
			return this;
		}

		public TextCorpusTokenizerTransformer AddTransformer<T>()
			where T : IRowProcessor<TextRow>
		{
			return AddTransformer(typeof(T).FullName!);
		}

		public TextCorpusTokenizerTransformer AddTransformer(string transformerClassName)
		{
			TokenizeTransformChain.Add($"Transform({transformerClassName})");
			return this;
		}

		public async Task<ITextCorpus> TokenizeTransform(IMediator mediator, CancellationToken cancellationToken)
		{
			var command = new TokenizeTextCorpusCommand(TextCorpus, TokenizeTransformChain);
			var result = await mediator.Send(command, cancellationToken);
			result.ThrowIfCanceledOrFailed(true);

			return result.Data!;
		}

		public ScriptureTextCorpus TextCorpus { get; private set; }
		public List<string> TokenizeTransformChain { get; }
	}
}
