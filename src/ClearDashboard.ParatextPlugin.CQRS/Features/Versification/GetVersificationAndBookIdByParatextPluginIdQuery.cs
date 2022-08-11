using ClearDashboard.DAL.CQRS;
using MediatR;
using SIL.Scripture;
using System;
using System.Collections.Generic;
using System.Text;
using ClearDashboard.DataAccessLayer.Models.Common;

namespace ClearDashboard.ParatextPlugin.CQRS.Features.Versification
{
    public record GetVersificationAndBookIdByParatextPluginIdQuery : IRequest<RequestResult<VersificationBookIds>>
    {        
        public GetVersificationAndBookIdByParatextPluginIdQuery(string paratextProjectId)
        {
            ParatextProjectId = paratextProjectId;
        }
        public string ParatextProjectId { get; }
    }
}
