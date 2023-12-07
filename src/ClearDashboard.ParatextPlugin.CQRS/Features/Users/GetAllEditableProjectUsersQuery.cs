using ClearDashboard.DAL.CQRS;
using ClearDashboard.DataAccessLayer.Models;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClearDashboard.ParatextPlugin.CQRS.Features.Users
{
    public record GetAllEditableProjectUsersQuery(string ParatextProjectId) : IRequest<RequestResult<List<string>>>
    {
        public string ParatextProjectId { get; } = ParatextProjectId;
    }
}
