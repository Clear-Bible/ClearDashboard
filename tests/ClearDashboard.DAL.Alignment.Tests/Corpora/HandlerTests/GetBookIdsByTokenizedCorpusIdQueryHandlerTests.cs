using ClearBible.Engine.Corpora;
using ClearBible.Engine.Tokenization;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Features.Corpora;
using SIL.Machine.Corpora;
using SIL.Machine.Tokenization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace ClearDashboard.DAL.Alignment.Tests.Corpora.HandlerTests;

public class GetBookIdsByTokenizedCorpusIdQueryHandlerTests : TestBase
{
    public GetBookIdsByTokenizedCorpusIdQueryHandlerTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    [Trait("Category", "Handlers")]
    public async void BookIds__CreateTokenizedCorpusThenQuery()
    {
        try
        {
            var textCorpus = TestDataHelpers.GetSampleTextCorpus();

            var command = new CreateTokenizedCorpusFromTextCorpusCommand(textCorpus, true, "NameX", "LanguageX", "BackTranslation", string.Empty);
            var commandResult = await Mediator.Send(command);

            var query = new GetBookIdsByTokenizedCorpusIdQuery(commandResult.Data?.TokenizedCorpusId!);
            var queryResult = await Mediator.Send(query);

            Assert.NotNull(queryResult);
            Assert.True(queryResult.Success);

            Output.WriteLine(queryResult.Data.corpusId.ToString());
            foreach (var bookId in queryResult.Data.bookIds)
            {
                Output.WriteLine($"\t{bookId}");
            }
        }
        finally
        {
            await DeleteDatabaseContext();
        }
    }

    [Fact]
    [Trait("Category", "Handlers")]
    public async void BookIds_QueryBogusId()
    {
        try
        {
            var query = new GetBookIdsByTokenizedCorpusIdQuery(new TokenizedCorpusId(Guid.NewGuid()));

            var result = await Mediator.Send(query);
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Message);
        }
        finally
        {
            await DeleteDatabaseContext();
        }
    }
}
