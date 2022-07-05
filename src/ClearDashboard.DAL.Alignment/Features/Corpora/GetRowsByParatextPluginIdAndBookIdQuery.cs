using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using MediatR;

namespace ClearDashboard.DAL.Alignment.Features.Corpora
{
    public record GetRowsByParatextPluginIdAndBookIdQuery(string ProjectName) : ProjectRequestQuery<IEnumerable<(string chapter, string verse, string text, bool isSentenceStart)>>(ProjectName)
    {
        public GetRowsByParatextPluginIdAndBookIdQuery(string projectName, string paratextPluginId, string bookId): this(projectName)
        {
            ParatextPluginId = paratextPluginId;
            BookId = bookId;
        }
        public string ParatextPluginId { get;  }
        public string BookId { get;  }
    }
}
