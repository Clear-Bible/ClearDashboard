using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using MediatR;
using SIL.Scripture;

namespace ClearDashboard.DAL.Alignment.Features.Corpora
{
    public record GetVersificationAndBookIdByParatextPluginIdQuery(string ProjectName) : ProjectRequestQuery<(ScrVers? versification, IEnumerable<string> bookAbbreviations)>(ProjectName)
    {
        public GetVersificationAndBookIdByParatextPluginIdQuery(string projectName, string paratextPluginId): this(projectName)
        {
            ParatextPluginId = paratextPluginId;
        }
        public string ParatextPluginId { get; }
    }
}
