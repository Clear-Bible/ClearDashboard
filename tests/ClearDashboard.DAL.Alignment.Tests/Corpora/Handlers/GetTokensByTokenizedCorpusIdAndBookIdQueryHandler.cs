using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using System.Linq;

using ClearDashboard.DAL.CQRS;
using ClearBible.Engine.Corpora;
using ClearBible.Alignment.DataServices.Features.Corpora;
using ClearBible.Engine.Tokenization;

using SIL.Machine.Corpora;
using SIL.Machine.Tokenization;
using SIL.Scripture;

namespace ClearBible.Alignment.DataServices.Tests.Corpora.Handlers
{
    public class GetTokensByTokenizedCorpusIdAndBookIdQueryHandler : IRequestHandler<
        GetTokensByTokenizedCorpusIdAndBookIdQuery,
        RequestResult<IEnumerable<(string chapter, string verse, IEnumerable<Token> tokens, bool isSentenceStart)>>>
    {
        public Task<RequestResult<IEnumerable<(string chapter, string verse, IEnumerable<Token> tokens, bool isSentenceStart)>>>
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
                    .Select(sg => new
                    {
                        Chapter = g.Key,
                        Verse = sg.Key,
                        Tokens = sg
                            .SelectMany(v => ((TokensTextRow)v).Tokens),
                        IsSentenceStart = sg
                            .First().IsSentenceStart
                    })
                )
                .Select(cvts => (cvts.Chapter.ToString(), cvts.Verse.ToString(), cvts.Tokens, cvts.IsSentenceStart));


            return Task.FromResult(
                new RequestResult<IEnumerable<(string chapter, string verse, IEnumerable<Token> tokens, bool isSentenceStart)>>
                (result: chapterVerseTokens,
                success: true,
                message: "successful result from test"));
        }
    }
}
