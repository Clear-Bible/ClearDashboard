using Caliburn.Micro;
using ClearDashboard.DAL.Alignment.Features.Translation;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using ClearDashboard.DataAccessLayer.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClearDashboard.DAL.Alignment.Features.Events
{
    public class AlignmentAddedRemovedEventHandler : INotificationHandler<AlignmentAddedRemovedEvent>
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly ILogger _logger;
        public AlignmentAddedRemovedEventHandler(
            ILogger<AlignmentAddedRemovedEventHandler> logger,
            IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            _logger = logger;
        }

        public async Task Handle(AlignmentAddedRemovedEvent notification, CancellationToken cancellationToken)
        {
            var sourceTexts = notification.AlignmentsRemoved.Select(a => a.SourceTokenComponent!.TrainingText!);
            sourceTexts = sourceTexts.Append(notification.AlignmentAdded.SourceTokenComponent!.TrainingText!).Distinct();

            notification.ProjectDbContext.AlignmentSetDenormalizationTasks.AddRange(sourceTexts.Select(st =>
                new AlignmentSetDenormalizationTask()
                {
                    AlignmentSetId = notification.AlignmentAdded.AlignmentSetId,
                    SourceText = st
                }));

            await notification.ProjectDbContext.SaveChangesAsync(cancellationToken);

            await _eventAggregator.PublishOnBackgroundThreadAsync(notification);
        }
    }
}
