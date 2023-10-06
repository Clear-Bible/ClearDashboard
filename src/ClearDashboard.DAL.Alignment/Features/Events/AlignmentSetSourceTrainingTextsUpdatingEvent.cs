using ClearDashboard.DataAccessLayer.Data;
using MediatR;

namespace ClearDashboard.DAL.Alignment.Features.Events
{
    public class AlignmentSetSourceTrainingTextsUpdatingEvent : INotification
    {
        public IDictionary<Guid, List<string>> SourceTrainingTextsByAlignmentSetId { get; }
        public ProjectDbContext ProjectDbContext { get; }

        /// <summary>
        /// Fired after the alignment set creation is complete
        /// </summary>
        /// <param name="alignmentSetId"></param>
        public AlignmentSetSourceTrainingTextsUpdatingEvent(IDictionary<Guid, List<string>> sourceTrainingTextsByAlignmentSetId, ProjectDbContext projectDbContext)
        {
            SourceTrainingTextsByAlignmentSetId = sourceTrainingTextsByAlignmentSetId;
            ProjectDbContext = projectDbContext;
        }
    }
}