using ClearDashboard.DAL.CQRS;
using ClearDashboard.DataAccessLayer.Models.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClearDashboard.ParatextPlugin.CQRS.Features.CheckUsfm
{
    public record GetCheckUsfmQuery(string Id) : IRequest<RequestResult<UsfmHelper>>
    {
        public string Id { get; } = Id;
    }
}
