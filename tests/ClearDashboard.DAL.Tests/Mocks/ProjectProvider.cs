using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.DAL.Tests.Mocks
{
    public  class ProjectProvider:  IProjectProvider
    {
        public ProjectInfo CurrentProject { get; set; }
    }
}
