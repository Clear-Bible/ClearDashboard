using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.WPF.Tests.Mocks
{
    internal class UserProvider : IUserProvider
    {
        public User? CurrentUser { get; set; }
    }
}
