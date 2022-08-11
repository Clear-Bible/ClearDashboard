using ClearDashboard.DAL.Alignment.Exceptions;
using ClearDashboard.DAL.Alignment.Features.Corpora;
using MediatR;
using SIL.Machine.Corpora;
using SIL.Scripture;

namespace ClearDashboard.DAL.Alignment.Corpora
{
    internal class ParatextPluginText : ScriptureText
    {
        private readonly string paratextPluginId_;
        private readonly IMediator mediator_;
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="parallelCorpusId">primary key of parallel corpus id db entity</param>
        /// <param name="bookId">book in three character SIL format</param>
        /// <param name-"isSource">if true, get source corpora, else target</param>
        /// <param name="versification"></param>
        //public Text(IMediator mediator, ParallelCorpusId parallelCorpusId, string bookId, bool isSource, ScrVers versification)
        //    : base(bookId, versification)
        //{
            //IMPLEMENTER'S NOTES:: needs to configure GetVersesInDocOrder() to ONLY return the text parallel related.
        //}

        public ParatextPluginText(string paratextPluginId, IMediator mediator, ScrVers versification, string bookId)
            : base(bookId, versification)
        {
            paratextPluginId_ = paratextPluginId;
            mediator_ = mediator;
        }
        protected override IEnumerable<TextRow> GetVersesInDocOrder()
        {
            var command = new GetRowsByParatextProjectIdAndBookIdQuery(paratextPluginId_, Id);  //Note that in ScriptureText Id is the book abbreviation bookId.

            var result = Task.Run(() => mediator_.Send(command)).GetAwaiter().GetResult();
            if (result.Success)
            {
                var verses = result.Data;

                if (verses == null)
                    throw new MediatorErrorEngineException("GetTextRowsByParatextPluginIdAndBookIdQuery returned null data");

                return verses
                    .SelectMany(verse => CreateRows(verse.chapter, verse.verse, verse.text, verse.isSentenceStart));

            }
            else
            {
                throw new MediatorErrorEngineException(result.Message);
            }
        }
    }
}
