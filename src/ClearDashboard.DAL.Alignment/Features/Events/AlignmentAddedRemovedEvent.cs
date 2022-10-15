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
        public AlignmentAddedRemovedEvent(IEnumerable<Models.Alignment> alignmentsRemoved, Models.Alignment alignmentAdded, ProjectDbContext projectDbContext)
        {
            AlignmentsRemoved = alignmentsRemoved;
            AlignmentAdded = alignmentAdded;
            ProjectDbContext = projectDbContext;
        }
    }
}