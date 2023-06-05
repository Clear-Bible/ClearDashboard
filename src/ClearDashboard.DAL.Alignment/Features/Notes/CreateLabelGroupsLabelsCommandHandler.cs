using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

//USE TO ACCESS Models
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.DAL.Alignment.Features.Notes;

public class CreateLabelGroupsLabelsCommandHandler : ProjectDbContextCommandHandler<CreateLabelGroupsLabelsCommand,
    RequestResult<Unit>, Unit>
{
    private readonly IMediator _mediator;

    public CreateLabelGroupsLabelsCommandHandler(IMediator mediator,
        ProjectDbContextFactory? projectNameDbContextFactory, IProjectProvider projectProvider,
        ILogger<CreateLabelGroupsLabelsCommandHandler> logger) : base(projectNameDbContextFactory, projectProvider,
        logger)
    {
        _mediator = mediator;
    }

    protected override async Task<RequestResult<Unit>> SaveDataAsync(CreateLabelGroupsLabelsCommand request,
        CancellationToken cancellationToken)
    {
        // LabelGroup.Import INSERTS Label and LabelGroup only when one doesn’t exist with
        // the same name, regardless of whether templateText is different, i.e. import
        // never updates but instead ignores import label records where a label with the same Text already exists.

        if (request.LabelGroupNamesLabels is null || !request.LabelGroupNamesLabels.Any())
        {
            return new RequestResult<Unit>(Unit.Value);
        }

        var labelGroupNames = request.LabelGroupNamesLabels.Keys;
        var labelDataPairs = request.LabelGroupNamesLabels
            .SelectMany(e => e.Value)
            .DistinctBy(e => e.Text);

        var existingLabelGroups = ProjectDbContext!.LabelGroups
            .Include(e => e.Labels)
            .Where(e => labelGroupNames.Contains(e.Name))
            .ToList();
        var existingLabels = ProjectDbContext!.Labels
            .Include(e => e.LabelGroups)
            .Where(e => labelDataPairs
                .Select(e => e.Text).Contains(e.Text!))
            .ToList();

        var newLabelGroups = labelGroupNames
            .Except(existingLabelGroups.Select(e => e.Name))
            .Select(e =>
                new Models.LabelGroup
                {
                    Id = Guid.NewGuid(),
                    Name = e
                }
            );
        ProjectDbContext!.LabelGroups.AddRange(newLabelGroups);

        var newLabels = labelDataPairs
            .ExceptBy(existingLabels.Select(e => e.Text), e => e.Text)
            .Select(e =>
                new Models.Label
                {
                    Id = Guid.NewGuid(),
                    Text = e.Text,
                    TemplateText = e.TemplateText
                }
             );
        ProjectDbContext!.Labels.AddRange(newLabels);

        var labelGroupsByName = existingLabelGroups
            .Union(newLabelGroups)
            .ToDictionary(e => e.Name, e => e);
        var labelsByText = existingLabels
            .Union(newLabels)
            .ToDictionary(e => e.Text!, e => e);

        var identifiableEntityComparer = new IdentifiableEntityComparer();
        foreach (var kvp in request.LabelGroupNamesLabels)
        {
            var labelGroupInRequest = labelGroupsByName[kvp.Key];
            foreach (var (Text, TemplateText) in kvp.Value)
            {
                var labelInRequest = labelsByText[Text];
                if (!labelGroupInRequest.Labels.Contains(labelInRequest, identifiableEntityComparer))
                {
                    ProjectDbContext.LabelGroupAssociations.Add(new Models.LabelGroupAssociation
                    {
                        Id = Guid.NewGuid(),
                        Label = labelInRequest,
                        LabelGroup = labelGroupInRequest
                    });
                }
            }
        }

        _ = await ProjectDbContext!.SaveChangesAsync(cancellationToken);

        return new RequestResult<Unit>(Unit.Value);
    }
}