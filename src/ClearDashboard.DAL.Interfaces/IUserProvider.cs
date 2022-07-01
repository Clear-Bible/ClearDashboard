using ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.DAL.Interfaces
{
    public interface IUserProvider
    {
        User CurrentUser { get; set; }
    }

    public interface IProjectProvider
    {
        ProjectInfo CurrentProject { get; set; }
    }
}
