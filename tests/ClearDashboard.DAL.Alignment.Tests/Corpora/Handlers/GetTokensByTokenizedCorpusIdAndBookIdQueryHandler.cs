using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ClearBible.Engine.Corpora;
using ClearBible.Engine.Tokenization;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Features.Corpora;
using ClearDashboard.DAL.CQRS;
using MediatR;
using SIL.Machine.Corpora;
using SIL.Machine.Tokenization;
using SIL.Scripture;

namespace ClearDashboard.DAL.Alignment.Tests.Corpora.Handlers
{
    public class GetTokensByTokenizedCorpusIdAndBookIdQueryHandler : IRequestHandler<
        GetTokensByTokenizedCorpusIdAndBookIdQuery,
        RequestResult<IEnumerable<VerseTokens>>>
    {
        public Task<RequestResult<IEnumerable<VerseTokens>>>
            Handle(GetTokensByTokenizedCorpusIdAndBookIdQuery command, CancellationToken cancellationToken)
        {
            //DB Impl notes: look at command.TokenizedCorpusId and find in TokenizedCorpus table.
            //Then iterate tokens and package them by verse then return enumerable.

            var corpus = new UsfmFileTextCorpus("usfm.sty", Encoding.UTF8, TestDataHelpers.UsfmTestProjectPath)
                .Tokenize<LatinWordTokenizer>()
                .Transform<IntoTokensTextRowProcessor>();

            var chapterVerseTokens = corpus.GetRows(new List<string>() { command.BookId })
                .GroupBy(r => ((VerseRef)r.Ref).ChapterNum)
                .OrderBy(g => g.Key)
                .SelectMany(g => g
                    .GroupBy(sg => ((VerseRef)sg.Ref).VerseNum)
                    .OrderBy(sg => sg.Key)
                    .Select(sg =>
                    {
                        var textRow = sg.First();

                        return new
                        {
                            Chapter = g.Key,
                            Verse = sg.Key,
                            Tokens = sg
                                .SelectMany(v => ((TokensTextRow)v).Tokens),
                            textRow.IsSentenceStart,
                            textRow.IsInRange,
                            textRow.IsRangeStart,
                            textRow.IsEmpty,
                            textRow.OriginalText
                        };
                    })
                )
                .Select(cvts => new VerseTokens(
                    cvts.Chapter.ToString(), 
                    cvts.Verse.ToString(), 
                    cvts.Tokens, 
                    cvts.IsSentenceStart,
                    cvts.IsInRange,
                    cvts.IsRangeStart,
                    cvts.IsEmpty,
                    cvts.OriginalText));


            return Task.FromResult(
                new RequestResult<IEnumerable<VerseTokens>>
                (result: chapterVerseTokens,
                success: true,
                message: "successful result from test"));
        }
    }
}
