using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.DataAccessLayer.Features.Projects;

public record UpdateProjectCommand(Models.Project project) : ProjectRequestCommand<bool>;

public class UpdateProjectCommandHandler : ProjectDbContextCommandHandler<UpdateProjectCommand, RequestResult<bool>, bool>
{
    private readonly IMediator _mediator;
    public UpdateProjectCommandHandler(IMediator mediator, ProjectDbContextFactory? projectNameDbContextFactory,
        IProjectProvider projectProvider, ILogger<UpdateProjectCommandHandler> logger)
        : base(projectNameDbContextFactory, projectProvider, logger)
    {
        _mediator = mediator;
    }

    protected override async Task<RequestResult<bool>> SaveDataAsync(UpdateProjectCommand request, CancellationToken cancellationToken)
    {
        var projectAssets = await ProjectNameDbContextFactory.Get(request.project.ProjectName);

        Logger.LogInformation($"Saving the design surface layout for {request.project.ProjectName}");
        projectAssets.ProjectDbContext.Attach(request.project);

        await projectAssets.ProjectDbContext.SaveChangesAsync();
        return new RequestResult<bool>(true);
    }
}