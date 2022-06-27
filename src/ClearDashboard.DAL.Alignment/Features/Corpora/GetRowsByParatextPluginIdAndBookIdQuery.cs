using ClearDashboard.DAL.CQRS;
using MediatR;

namespace ClearDashboard.DAL.Alignment.Features.Corpora
{
    public record GetRowsByParatextPluginIdAndBookIdQuery : IRequest<RequestResult<IEnumerable<(string chapter, string verse, string text, bool isSentenceStart)>>>
    {
        public GetRowsByParatextPluginIdAndBookIdQuery(string paratextPluginId, string bookId)
        {
            ParatextPluginId = paratextPluginId;
            BookId = bookId;
        }
        public string ParatextPluginId { get;  }
        public string BookId { get;  }
    }
}
