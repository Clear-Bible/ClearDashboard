using ClearBible.Engine.Corpora;
using ClearBible.Engine.Tokenization;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Exceptions;
using ClearDashboard.DAL.Alignment.Notes;
using ClearDashboard.ParatextPlugin.CQRS.Features.Notes;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Xunit;
using Xunit.Abstractions;
using ClearDashboard.DataAccessLayer.Models;
using Token = ClearBible.Engine.Corpora.Token;
using SIL.Machine.Corpora;
using SIL.Machine.Tokenization;
using static ClearBible.Engine.Persistence.FileGetBookIds;
using ClearBible.Engine.Exceptions;
using SIL.Scripture;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System;
using MediatR;

namespace ClearDashboard.DAL.Alignment.Tests.Corpora
{
    public class GetNotesTests: TestBase
    {
        public GetNotesTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        [Trait("Requires", "Paratext ZZ_SUR on test machine, paratextprojectid 2d2be644c2f6107a5b911a5df8c63dc69fa4ef6f")]
        public async void GetNotesForChapter()
        {
            try
            {
                CancellationTokenSource cancellationSource = new CancellationTokenSource();
                CancellationToken cancellationToken = cancellationSource.Token;


                var textCorpus = (await ParatextProjectTextCorpus.Get(Mediator!, "2d2be644c2f6107a5b911a5df8c63dc69fa4ef6f", null, cancellationToken))
                    .Tokenize<LatinWordTokenizer>()
                    .Transform<IntoTokensTextRowProcessor>();

                var tokenTextRows = textCorpus.GetRows(new List<string>() { "GEN" })
                    .Where(r => ((VerseRef)r.Ref).ChapterNum == 1)
                    .Cast<TokensTextRow>()
                    .ToList();

                var getNotesCommandParam = new GetNotesQueryParam()
                {
                    ExternalProjectId = "2d2be644c2f6107a5b911a5df8c63dc69fa4ef6f",
                    BookNumber = 1,
                    ChapterNumber = 1,
                    IncludeResolved = true
                };

                var result = await Mediator!.Send(new GetNotesQuery(getNotesCommandParam), cancellationToken);
                if (result.Success)
                {
                    Assert.NotNull(result.Data);
                    
                    var externalNotesGroupedByVerse = result?.Data
                        ?.GroupBy(e => e.VerseRefString)
                            ?? throw new InvalidDataEngineException(name: "result.Data", value: "null", message: "result of GetNotesQuery is successful yet result.Data is null");

                    var verseNoteWithTokenIds = externalNotesGroupedByVerse
                        ?.Select(g => g
                            .Select(r => r)
                            .AddVerseAndTokensContext(tokenTextRows.First(ttr => ((VerseRef)ttr.Ref).ToString() == g.Key), new EngineStringDetokenizer(new LatinWordDetokenizer())))
                        .SelectMany(r => r)
                        .ToList()
                            ?? throw new InvalidDataEngineException(name: "result.Data", value: "null", message: "result of GetNotesQuery is successful yet result.Data is null");
                }
                else
                {
                    throw new MediatorErrorEngineException(result.Message);
                }
            }
            finally
            {
                await DeleteDatabaseContext();
            }
        }

        [Fact]
        [Trait("Requires", "Paratext ZZ_SUR on test machine, paratextprojectid 2d2be644c2f6107a5b911a5df8c63dc69fa4ef6f")]
        public async void ResolveExternalNote()
        {
            try
            {
                CancellationTokenSource cancellationSource = new CancellationTokenSource();
                CancellationToken cancellationToken = cancellationSource.Token;

                var getNotesCommandParam = new GetNotesQueryParam()
                {
                    ExternalProjectId = "2d2be644c2f6107a5b911a5df8c63dc69fa4ef6f",
                    BookNumber = 1,
                    ChapterNumber = 1,
                    IncludeResolved = true
                };

                var resultGetNotes = await Mediator!.Send(new GetNotesQuery(getNotesCommandParam), cancellationToken);
                if (!resultGetNotes.Success)
                {
                    Assert.Fail("get external notes failed.");
                }

                if ((resultGetNotes.Data?.Count() ?? 0) < 1 )
                {
                    Assert.Fail("null or zero external notes returned.");
                }

                var resultResolveExternalNoteCommand = await Mediator!.Send(new ResolveExternalNoteCommand(new ResolveExternalNoteCommandParam()
                {
                    ExternalProjectId = "2d2be644c2f6107a5b911a5df8c63dc69fa4ef6f",
                    ExternalNoteId = resultGetNotes.Data![1].ExternalNoteId,
                    VerseRefString = resultGetNotes.Data![1].VerseRefString
                }), cancellationToken);

                Assert.True(resultResolveExternalNoteCommand.Success);
            }
            finally
            {
                await DeleteDatabaseContext();
            }
        }

        [Fact]
        [Trait("Requires", "Paratext ZZ_SUR on test machine, paratextprojectid 2d2be644c2f6107a5b911a5df8c63dc69fa4ef6f")]
        public async void AddNewCommentToExternalNote()
        {
            try
            {
                CancellationTokenSource cancellationSource = new CancellationTokenSource();
                CancellationToken cancellationToken = cancellationSource.Token;

                var getNotesCommandParam = new GetNotesQueryParam()
                {
                    ExternalProjectId = "2d2be644c2f6107a5b911a5df8c63dc69fa4ef6f",
                    BookNumber = 1,
                    ChapterNumber = 1,
                    IncludeResolved = true
                };

                var resultGetNotes = await Mediator!.Send(new GetNotesQuery(getNotesCommandParam), cancellationToken);
                if (!resultGetNotes.Success)
                {
                    Assert.Fail("get external notes failed.");
                }

                if ((resultGetNotes.Data?.Count() ?? 0) < 1)
                {
                    Assert.Fail("null or zero external notes returned.");
                }

                var result = await Mediator!.Send(new AddNewCommentToExternalNoteCommand(new AddNewCommentToExternalNoteCommandParam()
                {
                    ExternalProjectId = "2d2be644c2f6107a5b911a5df8c63dc69fa4ef6f",
                    ExternalNoteId = resultGetNotes.Data![1].ExternalNoteId,
                    VerseRefString = resultGetNotes.Data![1].VerseRefString,
                    Comment = $"Another comment at {DateTime.Now}",
                    AssignToUserName = "josie"
                }), cancellationToken);


                Assert.True(result.Success);
            }
            finally
            {
                await DeleteDatabaseContext();
            }
        }
        [Fact]
        [Trait("Requires", "Paratext ZZ_SUR on test machine, paratextprojectid 2d2be644c2f6107a5b911a5df8c63dc69fa4ef6f")]
        public async void GetParatextLabels()
        {
            try
            {
                CancellationTokenSource cancellationSource = new CancellationTokenSource();
                CancellationToken cancellationToken = cancellationSource.Token;

                var getExternalLabelsQueryParam = new GetExternalLabelsQueryParam()
                {
                    ExternalProjectId = "2d2be644c2f6107a5b911a5df8c63dc69fa4ef6f",
                };

                var resultGetLabels = await Mediator!.Send(new GetExternalLabelsQuery(getExternalLabelsQueryParam), cancellationToken);
                if (!resultGetLabels.Success)
                {
                    Assert.Fail("get external labels failed.");
                }

                if ((resultGetLabels.Data?.Count() ?? 0) < 1)
                {
                    Assert.Fail("null or zero external labels returned.");
                }
            }
            finally
            {
                await DeleteDatabaseContext();
            }
        }
    }
}
