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
    public class AlignmentAddingRemovingEvent : INotification
    {
        public IEnumerable<Models.Alignment> AlignmentsRemoving { get; }
        public Models.Alignment AlignmentAdding { get; }
        public ProjectDbContext ProjectDbContext { get; }

        /// <summary>
        /// Fired during alignment set adding / removing
        /// ProjectDbContext is provided to this event so that the database operations
        /// of its handler can be done in the same transaction as the event publisher.
        /// </summary>
        /// <param name="alignmentsRemoving"></param>
        /// <param name="alignmentAdding"></param>
        /// <param name="projectDbContext"></param>
        public AlignmentAddingRemovingEvent(IEnumerable<Models.Alignment> alignmentsRemoving, Models.Alignment alignmentAdding, ProjectDbContext projectDbContext)
        {
            AlignmentsRemoving = alignmentsRemoving;
            AlignmentAdding = alignmentAdding;
            ProjectDbContext = projectDbContext;
        }
    }
}