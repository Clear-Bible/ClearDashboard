using System;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboardJsApi
{
	public class ApiUserProvider : IUserProvider
	{
        public User CurrentUser { get; set; }

		public ApiUserProvider()
		{
			CurrentUser = new User
			{
				Id = Guid.Parse("37C65E3C-90EE-45DF-BE39-98D4E01C7EF4"),
				FirstName = "Joe",
				LastName = "Blow"
			};
		}
    }
}

