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
    public class AlignmentSetCreatedEvent : INotification
    {
        public Guid AlignmentSetId { get; }
        public ProjectDbContext ProjectDbContext { get; }
        public AlignmentSetCreatedEvent(Guid alignmentSetId, ProjectDbContext projectDbContext)
        {
            AlignmentSetId = alignmentSetId;
            ProjectDbContext = projectDbContext;
        }
    }
}