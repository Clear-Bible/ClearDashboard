using ClearDashboard.DAL.CQRS.Features;
using SIL.Scripture;

namespace ClearDashboard.DAL.Alignment.Features.Corpora
{
    public record GetVersificationAndBookIdByParatextProjectIdQuery : ProjectRequestQuery<(ScrVers? versification,
        IEnumerable<string> bookAbbreviations)>
    {
        public GetVersificationAndBookIdByParatextProjectIdQuery(string paratextProjectId)
        {
            ParatextProjectId = paratextProjectId;
        }
        public string ParatextProjectId { get; }
    }
}
