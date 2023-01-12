using ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.DAL.Interfaces
{
    public interface IUserProvider
    {
        User? CurrentUser { get; set; }
        //string ParatextUserName { get; set; }
    }
}
