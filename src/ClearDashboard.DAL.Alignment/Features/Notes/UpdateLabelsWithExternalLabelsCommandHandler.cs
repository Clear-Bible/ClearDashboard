using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using Microsoft.Extensions.Logging;

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
            // FIXME: Chris implement
            // 1.adds label $"External_{request.Data.ProjectName}" label group if it doesn't exist. If it does exist, removes all associations. 
            // 2. adds all request.Data.ExternalLabel.Where(el => !labels.Select(l => l.Text).Contains(el.Text) (where ExternalLabel.Text is not already in a Label.Text.)
            // 3. associates all Labels.Where(l => request.Data.ExternalLabelTexts.Contains(l.Text)) with $"External_{request.Data.ProjectName}" label group

            return new RequestResult<bool>(true);
        }
    }
}
