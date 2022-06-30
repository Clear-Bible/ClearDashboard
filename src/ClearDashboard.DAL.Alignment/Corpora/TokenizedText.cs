using ClearBible.Engine.Corpora;
using ClearBible.Engine.Exceptions;
using ClearDashboard.DAL.Alignment.Features.Corpora;
using MediatR;
using SIL.Machine.Corpora;
using SIL.Scripture;

namespace ClearDashboard.DAL.Alignment.Corpora
{
    internal class TokenizedText : ScriptureText
    {
        private readonly TokenizedCorpusId tokenizedCorpusId_;
        private readonly IMediator mediator_;
        private readonly string projectName_;

        public TokenizedText(TokenizedCorpusId tokenizedCorpusId, IMediator mediator, ScrVers versification, string bookId, string projectName)
            : base(bookId, versification)
        {
            tokenizedCorpusId_ = tokenizedCorpusId;
            mediator_ = mediator;
            projectName_ = projectName;
        }
        protected override IEnumerable<TextRow> GetVersesInDocOrder()
        {
            var command = new GetTokensByTokenizedCorpusIdAndBookIdQuery(projectName_, tokenizedCorpusId_, Id); //Note that in ScriptureText Id is the book abbreviation bookId.

            var result = Task.Run(() => mediator_.Send(command)).GetAwaiter().GetResult();
            if (result.Success)
            {
                var verses = result.Data;

                if (verses == null)
                    throw new MediatorErrorEngineException("GetTokensByTokenizedCorpusIdAndBookIdQuery returned null data");

                return verses
                    .SelectMany(verse => CreateRows(verse.chapter, verse.verse, "", verse.isSentenceStart) // text parameter is set by TokensTextRow from the tokens
                        .Select(tr => new TokensTextRow(tr, verse.tokens.ToList()))); //MUST return TokensTextRow. 
            }
            else
            {
                throw new MediatorErrorEngineException(result.Message);
            }
        }
    }
}
