using ClearDashboard.DAL.CQRS;
using ClearDashboard.DataAccessLayer.Models;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClearDashboard.ParatextPlugin.CQRS.Features.Users
{
    public record GetAllEditableProjectUsersQuery() : IRequest<RequestResult<List<string>>>;
}
