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
    public class AlignmentSetSourceTokenIdsUpdatedEvent : INotification
    {
        public Guid AlignmentSetId { get; }
        public IEnumerable<Guid> SourceTokenIds { get; }

        /// <summary>
        /// Fired after the alignment set creation is complete
        /// </summary>
        /// <param name="alignmentSetId"></param>
        public AlignmentSetSourceTokenIdsUpdatedEvent(Guid alignmentSetId, IEnumerable<Guid> sourceTokenIds)
        {
            AlignmentSetId = alignmentSetId;
            SourceTokenIds = sourceTokenIds;
        }
    }
}