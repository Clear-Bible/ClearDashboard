using ClearDashboard.DAL.CQRS;
using MediatR;
using SIL.Scripture;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClearDashboard.ParatextPlugin.CQRS.Features.Versification
{
    public record GetVersificationAndBookIdByParatextPluginIdQuery : IRequest<RequestResult<(ScrVers? versification, IEnumerable<string> bookAbbreviations)>>
    {        
        public GetVersificationAndBookIdByParatextPluginIdQuery(string paratextProjectId)
        {
            ParatextProjectId = paratextProjectId;
        }
        public string ParatextProjectId { get; }
    }
}
