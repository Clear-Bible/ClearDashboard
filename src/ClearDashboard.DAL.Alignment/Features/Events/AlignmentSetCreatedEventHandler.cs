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
    public class AlignmentSetCreatedEventHandler : INotificationHandler<AlignmentSetCreatedEvent>
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly ILogger _logger;
        public AlignmentSetCreatedEventHandler(
            ILogger<AlignmentSetCreatedEventHandler> logger,
            IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            _logger = logger;
        }

        public async Task Handle(AlignmentSetCreatedEvent notification, CancellationToken cancellationToken)
        {
            notification.ProjectDbContext.AlignmentSetDenormalizationTasks.Add(new AlignmentSetDenormalizationTask()
            {
                AlignmentSetId = notification.AlignmentSetId
            });

            await notification.ProjectDbContext.SaveChangesAsync(cancellationToken);

            await _eventAggregator.PublishOnBackgroundThreadAsync(notification);
        }

    }
}
