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

		public ScriptureTextCorpus TextCorpus { get; private set; }
		public List<string> TokenizeTransformChain { get; }
	}

	public static class TokenizerTransformerExtentions
	{
		public static TextCorpusTokenizerTransformer AddTokenizer<T>(this ScriptureTextCorpus corpus)
			where T : ITokenizer<string, int, string>, new()
		{
			var tokenizerTransformer = new TextCorpusTokenizerTransformer(corpus);
			tokenizerTransformer.AddTokenizer<T>();

			return tokenizerTransformer;
		}

		public static TextCorpusTokenizerTransformer AddTokenizer(this ScriptureTextCorpus corpus, string tokenizerClassName)
		{
			var tokenizerTransformer = new TextCorpusTokenizerTransformer(corpus);
			tokenizerTransformer.AddTokenizer(tokenizerClassName);

			return tokenizerTransformer;
		}

	}
}
