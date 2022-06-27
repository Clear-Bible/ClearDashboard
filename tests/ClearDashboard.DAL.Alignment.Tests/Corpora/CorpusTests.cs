using ClearBible.Engine.Corpora;
using ClearBible.Engine.Tokenization;
using SIL.Machine.Corpora;
using SIL.Machine.Tokenization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using ClearBible.Alignment.DataServices.Corpora;
using MediatR;
using ClearBible.Alignment.DataServices.Tests.Corpora.Handlers;
using ClearBible.Alignment.DataServices.Tests.Tokenization;

namespace ClearBible.Alignment.DataServices.Tests.Corpora
{
    public class CorpusTests
    {
		protected readonly ITestOutputHelper output_;
		protected readonly IMediator mediator_;
		public CorpusTests(ITestOutputHelper output)
		{
			output_ = output;
			mediator_ = new MediatorMock(); //FIXME: inject mediator
		}

		[Fact]
		[Trait("Category", "Example")]
		public async void Corpus__ImportFromUsfm_SaveToDb()
		{
			//Import
			var corpus = new UsfmFileTextCorpus("usfm.sty", Encoding.UTF8, TestDataHelpers.UsfmTestProjectPath)
				.Tokenize<LatinWordTokenizer>()
				.Transform<IntoTokensTextRowProcessor>();

			//ITextCorpus.Create() extension requires that ITextCorpus source and target corpus have been transformed
			// into TokensTextRow, puts them into the DB, and returns a TokensTextRow.
			var tokenizedTextCorpus = await corpus.Create(mediator_, true, "NameX", "LanguageX", "LanguageType", ".Tokenize<LatinWordTokenizer>().Transform<IntoTokensTextRowProcessor>()");

			foreach (var tokensTextRow in tokenizedTextCorpus.Cast<TokensTextRow>())
			{
				//display verse info
				var verseRefStr = tokensTextRow.Ref.ToString();
				output_.WriteLine(verseRefStr);

				//display legacy segment
				var segmentText = string.Join(" ", tokensTextRow.Segment);
				output_.WriteLine($"segmentText: {segmentText}");

				//display tokenIds
				var tokenIds = string.Join(" ", tokensTextRow.Tokens.Select(t => t.TokenId.ToString()));
				output_.WriteLine($"tokenIds: {tokenIds}");

				//display tokens tokenized
				var tokensText = string.Join(" ", tokensTextRow.Tokens.Select(t => t.Text));
				output_.WriteLine($"tokensText: {tokensText}");

				//display tokens detokenized
				var detokenizer = new LatinWordDetokenizer();
				var tokensTextDetokenized = detokenizer.Detokenize(tokensTextRow.Tokens.Select(t => t.Text).ToList());
				output_.WriteLine($"tokensTextDetokenized: {tokensTextDetokenized}");
			}
		}

		[Fact]
		[Trait("Category", "Example")]
		public async void Corpus__GetAllCorpusVersionIds()
		{
			var corpusVersionIds = await TokenizedTextCorpus.GetAllCorpusVersionIds(mediator_);
			Assert.True(corpusVersionIds.Count() > 0);
		}

		[Fact]
		[Trait("Category", "Example")]
		public async void Corpus__GetAllTokenizedCorpusIds()
		{
			var corpusVersionIds = await TokenizedTextCorpus.GetAllCorpusVersionIds(mediator_);
			Assert.True(corpusVersionIds.Count() > 0);

			var tokenizedCorpusIds = await TokenizedTextCorpus.GetAllTokenizedCorpusIds(mediator_, corpusVersionIds.First().corpusVersionId);
			Assert.True(tokenizedCorpusIds.Count() > 0);
		}

		[Fact]
		[Trait("Category", "Example")]
		public async void Corpus__GetTokenizedTextCorpusFromDb()
		{
			var tokenizedTextCorpus = await TokenizedTextCorpus.Get(mediator_, new TokenizedCorpusId(new Guid()));

			foreach (var tokensTextRow in tokenizedTextCorpus.Cast<TokensTextRow>())
			{
				//display verse info
				var verseRefStr = tokensTextRow.Ref.ToString();
				output_.WriteLine(verseRefStr);

				//display legacy segment
				var segmentText = string.Join(" ", tokensTextRow.Segment);
				output_.WriteLine($"segmentText: {segmentText}");

				//display tokenIds
				var tokenIds = string.Join(" ", tokensTextRow.Tokens.Select(t => t.TokenId.ToString()));
				output_.WriteLine($"tokenIds: {tokenIds}");

				//display tokens tokenized
				var tokensText = string.Join(" ", tokensTextRow.Tokens.Select(t => t.Text));
				output_.WriteLine($"tokensText: {tokensText}");

				//display tokens detokenized
				var detokenizer = new LatinWordDetokenizer();
				var tokensTextDetokenized = detokenizer.Detokenize(tokensTextRow.Tokens.Select(t => t.Text).ToList());
				output_.WriteLine($"tokensTextDetokenized: {tokensTextDetokenized}");
			}
		}

		[Fact]
		[Trait("Category", "Example")]
		public async void Corpus__GetTokenizedTextCorpusFromDB_Change_SaveToDb()
		{
			var tokenizedTextCorpus = await TokenizedTextCorpus.Get(mediator_, new TokenizedCorpusId(new Guid()));

			Assert.Equal(16, tokenizedTextCorpus.Count());

			OnlyUpToCountTextRowProcessor.Train(2);

			var tokenizedTextCorpusWithOnlyTwoTokens = tokenizedTextCorpus
				.Filter<OnlyUpToCountTextRowProcessor>();

			Assert.Equal(2, tokenizedTextCorpusWithOnlyTwoTokens.Count());
		}

		[Fact]
		[Trait("Category", "Example")]
		public async void Corpus__GetTokenizedCorpus_byBook()
		{
			var tokenizedTextCorpus = await TokenizedTextCorpus.Get(mediator_, new TokenizedCorpusId(new Guid()));

			Assert.Equal(16, tokenizedTextCorpus.Count());
			Assert.Equal(4, tokenizedTextCorpus.GetRows(new List<string>() { "MRK" }).Count());

			foreach (var tokensTextRow in tokenizedTextCorpus.GetRows(new List<string>() { "MRK" }).Cast<TokensTextRow>())
			{
				//display verse info
				var verseRefStr = tokensTextRow.Ref.ToString();
				output_.WriteLine(verseRefStr);

				//display legacy segment
				var segmentText = string.Join(" ", tokensTextRow.Segment);
				output_.WriteLine($"segmentText: {segmentText}");

				//display tokenIds
				var tokenIds = string.Join(" ", tokensTextRow.Tokens.Select(t => t.TokenId.ToString()));
				output_.WriteLine($"tokenIds: {tokenIds}");

				//display tokens tokenized
				var tokensText = string.Join(" ", tokensTextRow.Tokens.Select(t => t.Text));
				output_.WriteLine($"tokensText: {tokensText}");

				//display tokens detokenized
				var detokenizer = new LatinWordDetokenizer();
				var tokensTextDetokenized = detokenizer.Detokenize(tokensTextRow.Tokens.Select(t => t.Text).ToList());
				output_.WriteLine($"tokensTextDetokenized: {tokensTextDetokenized}");
			}
		}
	}
}
