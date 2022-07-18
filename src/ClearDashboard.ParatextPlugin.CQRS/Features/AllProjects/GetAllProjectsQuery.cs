using System;
using System.Collections.Generic;
using System.Text;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DataAccessLayer.Models.Paratext;
using MediatR;
using Paratext.PluginInterfaces;

namespace ClearDashboard.ParatextPlugin.CQRS.Features.AllProjects
{
    public record GetAllProjectsQuery() : IRequest<RequestResult<List<IProject>>>;
}
