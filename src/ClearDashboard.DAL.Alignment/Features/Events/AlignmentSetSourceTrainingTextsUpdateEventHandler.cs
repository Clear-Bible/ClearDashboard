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
    public class AlignmentSetSourceTrainingTextsUpdateEventHandler : INotificationHandler<AlignmentSetSourceTrainingTextsUpdatingEvent>, INotificationHandler<AlignmentSetSourceTrainingTextsUpdatedEvent>
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly ILogger _logger;
        public AlignmentSetSourceTrainingTextsUpdateEventHandler(
            ILogger<AlignmentSetSourceTrainingTextsUpdateEventHandler> logger,
            IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            _logger = logger;
        }

        public async Task Handle(AlignmentSetSourceTrainingTextsUpdatingEvent notification, CancellationToken cancellationToken)
        {
            if (notification.SourceTrainingTextsByAlignmentSetId.Any())
            {
                notification.SourceTrainingTextsByAlignmentSetId.ToList().ForEach(e =>
                {
                    e.Value.ToList().ForEach(s =>
                    {
                        notification.ProjectDbContext.AlignmentSetDenormalizationTasks.Add(new AlignmentSetDenormalizationTask()
                        {
                            AlignmentSetId = e.Key,
                            SourceText = s
                        });
                    });
                });

                await notification.ProjectDbContext.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task Handle(AlignmentSetSourceTrainingTextsUpdatedEvent notification, CancellationToken cancellationToken)
        {
            await _eventAggregator.PublishOnBackgroundThreadAsync(notification, cancellationToken: cancellationToken);
        }

    }
}
