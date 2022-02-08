using ClearDashboard.Common.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClearDashboard.Common.Models
{
    public class DashboardProject
    {
        /// <summary>
        /// the target project
        /// </summary>
        private ParatextProject _TargetProject;
        [JsonProperty]
        public ParatextProject TargetProject
        {
            get => _TargetProject;
            set
            {
                _TargetProject = value;
                this.BaseTargetName = _TargetProject.Name;
                this.BaseTargetFullName = _TargetProject.FullName;
            }
        }

        /// <summary>
        /// list of LWC projects
        /// </summary>
        private List<ParatextProject> _lwcProjects;
        [JsonProperty]
        public List<ParatextProject> LWCProjects
        {
            get => _lwcProjects;
            set => _lwcProjects = value;
        }

        /// <summary>
        /// List of Back Translation projects
        /// </summary>
        private List<ParatextProject> _BTProjects;
        [JsonProperty]
        public List<ParatextProject> BTProjects
        {
            get => _BTProjects;
            set => _BTProjects = value;
        }

        /// <summary>
        /// The Paratext UserID of the creator
        /// </summary>
        private string _ParatextUser;
        [JsonProperty]
        public string ParatextUser
        {
            get => _ParatextUser;
            set => _ParatextUser = value;
        }

        /// <summary>
        /// Date that this project was created
        /// </summary>
        private DateTime _CreationDate;
        [JsonProperty]
        public DateTime CreationDate
        {
            get => _CreationDate;
            set => _CreationDate = value;
        }

        /// <summary>
        /// The Dashboard Project Name
        /// </summary>
        private string _Name;
        [JsonProperty]
        public string Name
        {
            get => _Name;
            set => _Name = value;
        }

        /// <summary>
        /// The Dashboard Project Name
        /// </summary>
        private string _BaseTargetName;
        [JsonProperty]
        public string BaseTargetName
        {
            get => _BaseTargetName;
            set => _BaseTargetName = value;
        }

        /// <summary>
        /// The Dashboard Project FullName
        /// </summary>
        private string _BaseTargetFullName;
        [JsonProperty]
        public string BaseTargetFullName
        {
            get => _BaseTargetFullName;
            set => _BaseTargetFullName = value;
        }
    }
}
