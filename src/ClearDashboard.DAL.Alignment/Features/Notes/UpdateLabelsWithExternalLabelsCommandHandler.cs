using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using Models = ClearDashboard.DataAccessLayer.Models;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace ClearDashboard.DAL.Alignment.Features.Notes
{
    public class UpdateLabelsWithExternalLabelsCommandHandler : ProjectDbContextCommandHandler<
        UpdateLabelsWithExternalLabelsCommand,
        RequestResult<bool>, 
        bool>
    {
        public UpdateLabelsWithExternalLabelsCommandHandler(
            ProjectDbContextFactory? projectNameDbContextFactory, IProjectProvider projectProvider,
            ILogger<UpdateLabelsWithExternalLabelsCommandHandler> logger) : base(projectNameDbContextFactory, projectProvider,
            logger)
        {
        }

        protected override async Task<RequestResult<bool>> SaveDataAsync(UpdateLabelsWithExternalLabelsCommand request, CancellationToken cancellationToken)
        {
            // 1.adds label $"External_{request.Data.ProjectName}" label group
            // if it doesn't exist. If it does exist, removes all associations. 

            var labelGroupNames = request.Data.ExternalLabels
                .Select(el => el.ExternalProjectName)
                .ToList();

            var labelGroupsDb = ProjectDbContext!.LabelGroups
                .Include(e => e.LabelGroupAssociations)
                .Where(e => labelGroupNames.Contains(e.Name))
                .ToDictionary(e => e.Name, e => e);

            var labelGroupsToAdd = labelGroupNames
                .Except(labelGroupsDb.Keys)
                .Select(e => new Models.LabelGroup
                {
                    Id = Guid.NewGuid(),
                    Name = e
                })
                .ToList();

            await ProjectDbContext.LabelGroups.AddRangeAsync(labelGroupsToAdd, cancellationToken);
            ProjectDbContext.LabelGroupAssociations.RemoveRange(labelGroupsDb.Values
                .SelectMany(e => e.LabelGroupAssociations));

            labelGroupsToAdd.ForEach(e => labelGroupsDb.Add(e.Name, e));

            // 2. adds all request.Data.ExternalLabel.Where(el => !labels.Select(l => l.Text).Contains(el.Text)
            // (where ExternalLabel.Text is not already in a Label.Text.)

            var labelTexts = request.Data.ExternalLabels
                .Select(el => el.ExternalText)
                .ToList();

            var labelTextsDb = ProjectDbContext.Labels
                .Where(e => labelTexts.Contains(e.Text!))
                .ToDictionary(e => e.Text!, e => e);

            var labelsToAdd = labelTexts
                .Except(labelTextsDb.Keys)
                .Select(e => new Models.Label
                {
                    Id = Guid.NewGuid(),
                    Text = e
                })
                .ToList();

            await ProjectDbContext.Labels.AddRangeAsync(labelsToAdd, cancellationToken);

            labelsToAdd.ForEach(e => labelTextsDb.Add(e.Text!, e));

            // 3. associates all Labels.Where(l => request.Data.ExternalLabelTexts.Contains(l.Text))
            // with $"External_{request.Data.ProjectName}" label group

            await ProjectDbContext.LabelGroupAssociations.AddRangeAsync(request.Data.ExternalLabels
                .Select(e => new Models.LabelGroupAssociation
                {
                    Id = Guid.NewGuid(),
                    LabelId = labelTextsDb[e.ExternalText].Id,
                    LabelGroupId = labelGroupsDb[e.ExternalProjectName].Id
                }), 
                cancellationToken);

            if (ProjectDbContext.ChangeTracker.HasChanges())
            {
                _ = await ProjectDbContext!.SaveChangesAsync(cancellationToken);
            }

            return new RequestResult<bool>(true);
        }
    }
}
