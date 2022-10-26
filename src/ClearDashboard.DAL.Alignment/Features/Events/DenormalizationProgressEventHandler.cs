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
using SIL.Machine.Utils;
using System.IO;

namespace ClearDashboard.DAL.Alignment.Features.Events
{
    public class DenormalizationProgressEventHandler : INotificationHandler<DenormalizationProgressEvent>
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly ILogger _logger;
        public DenormalizationProgressEventHandler(
            ILogger<DenormalizationProgressEventHandler> logger,
            IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            _logger = logger;
        }

        public async Task Handle(DenormalizationProgressEvent notification, CancellationToken cancellationToken)
        {
            if (notification.Exception is not null || notification.Description is not null)
            {
                _logger.LogInformation("Background task '{ProcessName}' reports status '{processStatus}' with message '{message}'", notification.Name, notification.Status, notification.Exception?.Message ?? notification.Description);
            }
            else
            {
                _logger.LogInformation("Background task '{ProcessName}' reports status '{processStatus}'", notification.Name, notification.Status);
            }

            var backgroundTaskStatus = new BackgroundTaskStatus
            {
                Name = notification.Name,
                EndTime = DateTime.Now,
                Description = notification.Description ?? null,
                ErrorMessage = notification.Exception != null ? $"{notification.Exception}" : null,
                TaskLongRunningProcessStatus = notification.Status
            };
            await _eventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(backgroundTaskStatus), cancellationToken);
        }
    }
}
