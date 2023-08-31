using MediatR;

namespace ClearDashboard.DAL.Alignment.Features.Events
{
    public class AlignmentSetSourceTrainingTextsUpdatedEvent : INotification
    {
        public IDictionary<Guid, List<string>> SourceTrainingTextsByAlignmentSetId { get; }

        /// <summary>
        /// Fired after the alignment set creation is complete
        /// </summary>
        /// <param name="alignmentSetId"></param>
        public AlignmentSetSourceTrainingTextsUpdatedEvent(IDictionary<Guid, List<string>> sourceTrainingTextsByAlignmentSetId)
        {
            SourceTrainingTextsByAlignmentSetId = sourceTrainingTextsByAlignmentSetId;
        }
    }
}