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

        /// <summary>
        /// Fired after the alignment set add / remove is complete
        /// </summary>
        /// <param name="alignmentsRemoved"></param>
        /// <param name="alignmentAdded"></param>
        public AlignmentAddedRemovedEvent(IEnumerable<Models.Alignment> alignmentsRemoved, Models.Alignment alignmentAdded)
        {
            AlignmentsRemoved = alignmentsRemoved;
            AlignmentAdded = alignmentAdded;
        }
    }
}