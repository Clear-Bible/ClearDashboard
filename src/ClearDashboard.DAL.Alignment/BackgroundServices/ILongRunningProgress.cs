using ClearDashboard.DataAccessLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClearDashboard.DAL.Alignment.BackgroundServices
{
    public interface ILongRunningProgress<T> : IProgress<T>
    {
        public void ReportCompleted(string? description = null);
        public void ReportException(Exception exception);
        public void ReportCancelRequestReceived(string? description = null);
    }
}
