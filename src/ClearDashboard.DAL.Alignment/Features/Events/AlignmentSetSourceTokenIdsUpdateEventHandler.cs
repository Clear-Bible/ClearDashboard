using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using ClearDashboard.DataAccessLayer.Models;
using MediatR;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Models = ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DAL.Alignment.Translation;
using Caliburn.Micro;

namespace ClearDashboard.DAL.Alignment.Features.Events
{
    public class AlignmentSetSourceTokenIdsUpdateEventHandler : INotificationHandler<AlignmentSetSourceTokenIdsUpdatingEvent>, INotificationHandler<AlignmentSetSourceTokenIdsUpdatedEvent>
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly ILogger _logger;
        public AlignmentSetSourceTokenIdsUpdateEventHandler(
            ILogger<AlignmentSetCreateEventHandler> logger,
            IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            _logger = logger;
        }

        public async Task Handle(AlignmentSetSourceTokenIdsUpdatingEvent notification, CancellationToken cancellationToken)
        {
            if (notification.SourceTokenIds.Any())
            {
                // In case the event thrower only provided SourceTokenComponentId for each alignment:
                var sourceTokenComponents = notification.ProjectDbContext.TokenComponents
                    .Where(e => notification.SourceTokenIds.Distinct().Contains(e.Id))
                    .ToList();

                sourceTokenComponents.ToList().ForEach(e =>
                {
                    notification.ProjectDbContext.AlignmentSetDenormalizationTasks.Add(new AlignmentSetDenormalizationTask()
                    {
                        AlignmentSetId = notification.AlignmentSetId,
                        SourceText = e.TrainingText
                    });
                });

                await notification.ProjectDbContext.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task Handle(AlignmentSetSourceTokenIdsUpdatedEvent notification, CancellationToken cancellationToken)
        {
            await _eventAggregator.PublishOnBackgroundThreadAsync(notification, cancellationToken: cancellationToken);
        }

    }
}
