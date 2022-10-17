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
    public class AlignmentAddedRemovedEvent : INotification
    {
        public IEnumerable<Models.Alignment> AlignmentsRemoved { get; }
        public Models.Alignment AlignmentAdded { get; }
        public ProjectDbContext ProjectDbContext { get; }

        /// <summary>
        /// ProjectDbContext is provided to this event so that the database operations
        /// of its handler can be done in the same transaction as the event publisher.
        /// </summary>
        /// <param name="alignmentsRemoved"></param>
        /// <param name="alignmentAdded"></param>
        /// <param name="projectDbContext"></param>
        public AlignmentAddedRemovedEvent(IEnumerable<Models.Alignment> alignmentsRemoved, Models.Alignment alignmentAdded, ProjectDbContext projectDbContext)
        {
            AlignmentsRemoved = alignmentsRemoved;
            AlignmentAdded = alignmentAdded;
            ProjectDbContext = projectDbContext;
        }
    }
}