using ClearBible.Engine.Corpora;
using ClearBible.Engine.Tokenization;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DataAccessLayer.Models.Common;
using ClearDashboard.DataAccessLayer.Threading;
using ClearDashboard.ParatextPlugin.CQRS.Features.Versification;
using SIL.Machine.Corpora;
using SIL.Machine.Tokenization;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace ClearDashboard.DAL.Alignment.Tests.Corpora
{
    public class AddNoteTests: TestBase
    {
        public AddNoteTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async void Foo()
        {
            try
            {
                CancellationTokenSource cancellationSource = new CancellationTokenSource();
                CancellationToken cancellationToken = cancellationSource.Token;

                var textCorpus = (await ParatextProjectTextCorpus.Get(Mediator!, "2d2be644c2f6107a5b911a5df8c63dc69fa4ef6f", cancellationToken))
                    .Tokenize<LatinWordTokenizer>()
                    .Transform<IntoTokensTextRowProcessor>();



                //ITextCorpus.Create() extension requires that ITextCorpus source and target corpus have been transformed
                // into TokensTextRow, puts them into the DB, and returns a TokensTextRow.
                var corpus = await Corpus.Create(Mediator!, true, "NameX", "LanguageX", "LanguageType", Guid.NewGuid().ToString());
                var tokenizedTextCorpus = await textCorpus.Create(Mediator!, corpus.CorpusId, "Unit Test", ".Tokenize<LatinWordTokenizer>().Transform<IntoTokensTextRowProcessor>()");

 
            }
            finally
            {
                await DeleteDatabaseContext();
            }
        }
    }
}
