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
    public class AlignmentAddRemoveEventHandler : INotificationHandler<AlignmentAddingRemovingEvent>, INotificationHandler<AlignmentAddedRemovedEvent>
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly ILogger _logger;
        public AlignmentAddRemoveEventHandler(
            ILogger<AlignmentAddRemoveEventHandler> logger,
            IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            _logger = logger;
        }

        public async Task Handle(AlignmentAddingRemovingEvent notification, CancellationToken cancellationToken)
        {
            if (notification.AlignmentAdding is null && !notification.AlignmentsRemoving.Any())
            {
                return;
            }

            var alignments = notification.AlignmentsRemoving.ToList();
            if (notification.AlignmentAdding is not null)
            {
                alignments.Add(notification.AlignmentAdding);
            }

            if (alignments.Any(a => a.SourceTokenComponent == null))
            {
                // In case the event thrower only provided SourceTokenComponentId for each alignment:
                var sourceTokenComponents = notification.ProjectDbContext.TokenComponents
                    .Where(e => alignments.Select(a => a.SourceTokenComponentId).Contains(e.Id))
                    .ToList();

                alignments.ForEach(a => a.SourceTokenComponent = sourceTokenComponents
                    .Where(t => t.Id == a.SourceTokenComponentId).FirstOrDefault());
            }

            var denormalizationTasks = alignments
                .Where(a => a.AlignmentSetId != Guid.Empty)
                .Where(a => a.SourceTokenComponent?.TrainingText != null)
                .GroupBy(a => new { a.AlignmentSetId, SourceText = a.SourceTokenComponent!.TrainingText! })
                .Select(g => new AlignmentSetDenormalizationTask()
                {
                    AlignmentSetId = g.Key.AlignmentSetId,
                    SourceText = g.Key.SourceText
                });

            notification.ProjectDbContext.AlignmentSetDenormalizationTasks.AddRange(denormalizationTasks);

            await notification.ProjectDbContext.SaveChangesAsync(cancellationToken);
        }
        public async Task Handle(AlignmentAddedRemovedEvent notification, CancellationToken cancellationToken)
        {
            await _eventAggregator.PublishOnBackgroundThreadAsync(notification, cancellationToken: cancellationToken);
        }
    }
}