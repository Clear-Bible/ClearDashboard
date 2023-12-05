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

            //unique label group names represented in external labels
            var labelGroupNamesInExternalLabels = request.Data.ExternalLabels
                .Select(el => $"External_{el.ExternalProjectName}")
                .Distinct()
                .ToList();

            //unique label group names represented in external labels already in database
            var labelGroupsAlreadyInDb = ProjectDbContext!.LabelGroups
                .Include(lg => lg.LabelGroupAssociations)
                .Where(e => labelGroupNamesInExternalLabels.Contains(e.Name))
                .ToDictionary(lg => lg.Name, e => e);

            //add only label group names represented in external labels not already in db
            var labelGroupsToAdd = labelGroupNamesInExternalLabels
                .Except(labelGroupsAlreadyInDb.Keys)
                .Select(str => new Models.LabelGroup
                {
                    Id = Guid.NewGuid(),
                    Name = str
                })
                .ToList();
            await ProjectDbContext.LabelGroups.AddRangeAsync(labelGroupsToAdd, cancellationToken);

            // for label groups represented in external labels not already in db, remove their associations.
            ProjectDbContext.LabelGroupAssociations.RemoveRange(labelGroupsAlreadyInDb.Values
                .SelectMany(lg => lg.LabelGroupAssociations));

            // add ones added to labelGroupNamesAlreadyInDb since they were added a couple lines back.
            labelGroupsToAdd.ForEach(lg => labelGroupsAlreadyInDb.Add(lg.Name, lg));

            // 2. adds all request.Data.ExternalLabel.Where(el => !labels.Select(l => l.Text).Contains(el.Text)
            // (where ExternalLabel.Text is not already in a Label.Text.)

            //unique label names represented in external labels
            var labelTextsInExternalLabels = request.Data.ExternalLabels
                .Select(el => el.ExternalText)
                .Distinct()
                .ToList();

            //unique label group names represented in external labels already in database
            var labelsAlreadyInDb = ProjectDbContext.Labels
                .Where(l => labelTextsInExternalLabels.Contains(l.Text!))
                .ToDictionary(l => l.Text!, e => e);

            //add only label names represented in external labels not already in db
            var labelsToAdd = labelTextsInExternalLabels
                .Except(labelsAlreadyInDb.Keys)
                .Select(str => new Models.Label
                {
                    Id = Guid.NewGuid(),
                    Text = str
                })
                .ToList();
            await ProjectDbContext.Labels.AddRangeAsync(labelsToAdd, cancellationToken);

            // add ones added to labelTextsAlreadyInDb since they were added a couple lines back.
            labelsToAdd.ForEach(l => labelsAlreadyInDb.Add(l.Text!, l));

            // 3. associates all Labels.Where(l => request.Data.ExternalLabelTexts.Contains(l.Text))
            // with $"External_{request.Data.ProjectName}" label group

            await ProjectDbContext.LabelGroupAssociations.AddRangeAsync(request.Data.ExternalLabels
                .Select(el => new Models.LabelGroupAssociation
                {
                    Id = Guid.NewGuid(),
                    LabelId = labelsAlreadyInDb[el.ExternalText].Id,
                    LabelGroupId = labelGroupsAlreadyInDb[$"External_{el.ExternalProjectName}"].Id
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
