using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DataAccessLayer.Data;
using ClearDashboard.DataAccessLayer.Models;
using MediatR;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClearDashboard.DataAccessLayer.Threading;
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.DAL.Alignment.Features.Events
{
    public class DenormalizationProgressEvent : INotification
    {
        public LongRunningTaskStatus Status { get; }
        public string Name { get; }
        public string? Description { get; }
        public Exception? Exception { get; }
 
        public DenormalizationProgressEvent(LongRunningTaskStatus status, string name, string? description = null, Exception? exception = null)
        {
            Status = status;
            Name = name;
            Description = description;
            Exception = exception;
        }
    }
}