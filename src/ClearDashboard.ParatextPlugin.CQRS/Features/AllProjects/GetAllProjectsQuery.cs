using ClearDashboard.DAL.CQRS;
using ClearDashboard.DataAccessLayer.Models;
using MediatR;
using System.Collections.Generic;

namespace ClearDashboard.ParatextPlugin.CQRS.Features.AllProjects
{
    public record GetAllProjectsQuery() : IRequest<RequestResult<List<ParatextProject>>>;
}
