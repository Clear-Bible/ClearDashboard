using ClearDashboard.DAL.CQRS.Features;
using SIL.Scripture;

namespace ClearDashboard.DAL.Alignment.Features.Corpora
{
    public record GetVersificationAndBookIdByParatextPluginIdQuery : ProjectRequestQuery<(ScrVers? versification, IEnumerable<string> bookAbbreviations)>
    {
        public GetVersificationAndBookIdByParatextPluginIdQuery(string paratextPluginId)
        {
            ParatextPluginId = paratextPluginId;
        }
        public string ParatextPluginId { get; }
    }
}
