using System;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboardJsApi
{
	public class ApiProjectProvider : IProjectProvider
	{
        public Project CurrentProject { get; set; }
        public ParatextProject CurrentParatextProject { get; set; }

        public ApiProjectProvider()
		{
            CurrentProject = new Project
            {
                Id = Guid.Parse("C7499E1A-E197-4E6E-BB4C-1DE0EF41F3D7"),
                ProjectName = "JsApiExperiment",
                IsRtl = false,
                UserId = Guid.Parse("37C65E3C-90EE-45DF-BE39-98D4E01C7EF4"),
                AppVersion = "1.2.0.11"
            };

            CurrentParatextProject = new ParatextProject
            {
                Guid = null
            };
		}

        public bool HasCurrentProject => CurrentProject.ProjectName != null;

        public bool HasCurrentParatextProject => CurrentParatextProject.Guid != null;

        public bool CanRunDenormalization => false;
    }
}

