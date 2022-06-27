using ClearBible.Alignment.DataServices.Corpora;
using ClearDashboard.DAL.CQRS;
using MediatR;
using SIL.Scripture;

namespace ClearBible.Alignment.DataServices.Features.Corpora
{
    public record GetVersificationAndBookIdByParatextPluginIdQuery : IRequest<RequestResult<(ScrVers? versification, IEnumerable<string> bookAbbreviations)>>
    {
        public GetVersificationAndBookIdByParatextPluginIdQuery(string paratextPluginId)
        {
            ParatextPluginId = paratextPluginId;
        }
        public string ParatextPluginId { get; }
    }
}
