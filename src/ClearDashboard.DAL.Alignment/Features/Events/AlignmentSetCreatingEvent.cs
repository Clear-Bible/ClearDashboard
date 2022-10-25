using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DataAccessLayer.Data;
using MediatR;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.DAL.Alignment.Features.Events
{
    public class AlignmentSetCreatingEvent : INotification
    {
        public Guid AlignmentSetId { get; }
        public ProjectDbContext ProjectDbContext { get; }

        /// <summary>
        /// Fired during alignment set creation
        /// ProjectDbContext is provided to this event so that the database operations
        /// of its handler can be done in the same transaction as the event publisher.
        /// </summary>
        /// <param name="alignmentSetId"></param>
        /// <param name="projectDbContext"></param>
        public AlignmentSetCreatingEvent(Guid alignmentSetId, ProjectDbContext projectDbContext)
        {
            AlignmentSetId = alignmentSetId;
            ProjectDbContext = projectDbContext;
        }
    }
}