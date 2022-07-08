using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.DAL.Tests.Mocks
{
    internal class UserProvider : IUserProvider
    {
        public User? CurrentUser { get; set; }
    }
}
