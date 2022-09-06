using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Exceptions;
using ClearDashboard.DAL.Alignment.Features.Corpora;
using MediatR;
using SIL.Machine.Corpora;
using SIL.Scripture;

namespace ClearDashboard.DAL.Alignment.Corpora
{
    internal class TokenizedText : ScriptureText
    {
        private readonly TokenizedTextCorpusId tokenizedCorpusId_;
        private readonly IMediator mediator_;

        public TokenizedText(TokenizedTextCorpusId tokenizedCorpusId, IMediator mediator, ScrVers versification, string bookId)
            : base(bookId, versification)
        {
            tokenizedCorpusId_ = tokenizedCorpusId;
            mediator_ = mediator;
        }
        protected override IEnumerable<TextRow> GetVersesInDocOrder()
        {
            var command = new GetTokensByTokenizedCorpusIdAndBookIdQuery(tokenizedCorpusId_, Id); //Note that in ScriptureText Id is the book abbreviation bookId.

            var result = Task.Run(() => mediator_.Send(command)).GetAwaiter().GetResult();
            if (result.Success)
            {
                var verses = result.Data;

                if (verses == null)
                    throw new MediatorErrorEngineException("GetTokensByTokenizedCorpusIdAndBookIdQuery returned null data");

                return verses
                    .SelectMany(verse => CreateRows(verse.Chapter, verse.Verse, "", verse.IsSentenceStart) // text parameter is set by TokensTextRow from the tokens
                        .Select(tr => new TokensTextRow(tr, verse.Tokens.ToList()))); //MUST return TokensTextRow. 
            }
            else
            {
                throw new MediatorErrorEngineException(result.Message);
            }
        }
    }
}
