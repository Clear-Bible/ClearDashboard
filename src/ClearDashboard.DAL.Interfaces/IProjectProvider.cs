using ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.DAL.Interfaces;

public interface IProjectProvider
{
    ProjectInfo? CurrentProject { get; set; }
    ParatextProject? CurrentParatextProject { get; set; }
}