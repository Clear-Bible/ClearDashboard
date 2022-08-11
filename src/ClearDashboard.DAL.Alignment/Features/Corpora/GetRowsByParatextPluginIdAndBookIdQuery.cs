using ClearDashboard.DAL.CQRS.Features;

namespace ClearDashboard.DAL.Alignment.Features.Corpora
{
    public record GetRowsByParatextPluginIdAndBookIdQuery : ProjectRequestQuery<IEnumerable<(string chapter, string verse, string text, bool isSentenceStart)>>
    {
        public GetRowsByParatextPluginIdAndBookIdQuery(string paratextProjectId, string bookId)
        {
            ParatextProjectId = paratextProjectId;
            BookId = bookId;
        }
        public string ParatextProjectId { get;  }
        public string BookId { get;  }
    }
}
