using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Commands;
using ClearDashboard.DAL.Alignment.Features.Corpora;
using ClearDashboard.DAL.Alignment.Tests.Corpora;
using SIL.Machine.Tokenization;
using SIL.Scripture;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit;
using ClearDashboard.DAL.Alignment.Corpora;

namespace ClearDashboard.DAL.Alignment.Tests.ApiCommandTests
{
	public class CorporaCommandTests : TestBase
	{
		public CorporaCommandTests(ITestOutputHelper output) : base(output)
		{
		}

		[Fact]
		[Trait("Category", "ApiCommands")]
		public async void GetSampleRows()
		{
			try
			{
				//Import
				var textCorpus = TestDataHelpers.GetSampleTextCorpus();

				// Create the corpus in the database:
				var corpus = await Corpus.Create(Mediator!, true, "NameX", "LanguageX", "Standard", Guid.NewGuid().ToString());

				// Create the TokenizedCorpus + Tokens in the database:
				var tokenizationFunction = ".Tokenize<LatinWordTokenizer>().Transform<IntoTokensTextRowProcessor>()";
				var command = new CreateTokenizedCorpusFromTextCorpusCommand(textCorpus, corpus.CorpusId, "Unit Test", tokenizationFunction, ScrVers.Original);

				var result = await Mediator!.Send(command);
				Assert.NotNull(result);
				Assert.True(result.Success);

				var tokenizedTextCorpus = result.Data;

				Assert.NotNull(tokenizedTextCorpus);
				Assert.All(tokenizedTextCorpus, tc => Assert.IsType<TokensTextRow>(tc));

				ProjectDbContext!.ChangeTracker.Clear();

				var corpusDB = ProjectDbContext!.Corpa.Include(c => c.TokenizedCorpora).FirstOrDefault(c => c.Id == tokenizedTextCorpus!.TokenizedTextCorpusId.CorpusId.Id);

				// =================================================================================
				// To execute, you need an Autofac IComponentContext or IContainer or ILifetimeScope:
				// =================================================================================
				var rowsByVerseRange = await new GetVerseRangeTokensCommand
				{
					TokenizedCorpusId = tokenizedTextCorpus!.TokenizedTextCorpusId.Id,
					VerseRef = new VerseRef(40, 2, 4, tokenizedTextCorpus.Versification),
					NumberOfVersesInChapterBefore = 2,
					NumberOfVersesInChapterAfter = 5
				}.ExecuteAsync(Container!, System.Threading.CancellationToken.None);

				Assert.Equal(5, rowsByVerseRange.Rows.Count());
				Assert.Equal(1, rowsByVerseRange.IndexOfVerse);

				foreach (var tokensTextRow in rowsByVerseRange.Rows)
				{
					// FIXME:  Is it ok if Tokens is empty for a tokensTextRow?
					//                Assert.NotEmpty(tokensTextRow.Tokens);
					Assert.All(tokensTextRow.Tokens, t => Assert.NotNull(t.TokenId));

					//display verse info
					var verseRefStr = tokensTextRow.Ref.ToString();
					Output.WriteLine(verseRefStr);

					//display legacy segment
					var segmentText = string.Join(" ", tokensTextRow.Segment);
					Output.WriteLine($"segmentText: {segmentText}");

					//display tokenIds
					var tokenIds = string.Join(" ", tokensTextRow.Tokens.Select(t => t.TokenId.ToString()));
					Output.WriteLine($"tokenIds: {tokenIds}");

					//display tokens tokenized
					var tokensText = string.Join(" ", tokensTextRow.Tokens.Select(t => t.SurfaceText));
					Output.WriteLine($"tokensText: {tokensText}");

					//display tokens detokenized
					var detokenizer = new LatinWordDetokenizer();
					var tokensTextDetokenized =
						detokenizer.Detokenize(tokensTextRow.Tokens.Select(t => t.SurfaceText).ToList());
					Output.WriteLine($"tokensTextDetokenized: {tokensTextDetokenized}");
				}

			}
			finally
			{
				await DeleteDatabaseContext();
			}
		}
	}
}
