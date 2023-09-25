using ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.DAL.Interfaces;

public interface IProjectProvider
{
    Project? CurrentProject { get; set; }
    ParatextProject? CurrentParatextProject { get; set; }
    bool HasCurrentProject { get; }
    bool HasCurrentParatextProject { get;  }
    bool CanRunDenormalization { get; }
}