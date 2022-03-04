using ClearDashboard.Common.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace ClearDashboard.Common.Models
{
    public class DashboardProject : INotifyPropertyChanged
    {
        public string DirectoryPath { get; set; }
        public string ClearEngineDirectoryPath { get; set; }
        public bool HasJsonProjectName { get; set; } = false;


        private string _projectName;
        [JsonProperty]
        public string ProjectName
        {
            get => _projectName;
            set
            {
                if (_projectName == value) return;
                _projectName = value;
                OnPropertyChanged();
            }
        }

        private string _projectPath;
        [JsonProperty]
        public string ProjectPath
        {
            get => _projectPath;
            set
            {
                _projectPath = value;
                DirectoryPath = value;
                ClearEngineDirectoryPath = Path.Combine(DirectoryPath, "ClearEngine");
            }
        }

        private DateTime _lastChanged;
        [JsonProperty]
        public DateTime LastChanged
        {
            get => _lastChanged;
            set
            {
                if (_lastChanged == value) return;
                _lastChanged = value;
                OnPropertyChanged();
            }
        }

        private string _fullFilePath;
        [JsonProperty]
        public string FullFilePath
        {
            get => _fullFilePath;
            set
            {
                if (_fullFilePath == value) return;
                _fullFilePath = value;

                FileInfo fi = new FileInfo(_fullFilePath);
                ProjectPath = fi.DirectoryName;

                OnPropertyChanged();
            }
        }


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

        private string _shortFilePath;
        [JsonProperty]
        public string ShortFilePath
        {
            get => _shortFilePath;
            set
            {
                if (_shortFilePath == value) return;
                _shortFilePath = value;
                OnPropertyChanged();
            }
        }

        private string _jsonProjectName;
        [JsonProperty]
        public string JsonProjectName
        {
            get => _jsonProjectName;
            set
            {
                if (_jsonProjectName == value) return;
                _jsonProjectName = value;
                OnPropertyChanged();
            }
        }

        private int _userValidationLevel;
        [JsonProperty]
        public int UserValidationLevel
        {
            get => _userValidationLevel;
            set
            {
                _userValidationLevel = value;
                OnPropertyChanged();
            }
        }


        private int _lastContentWordLevel;

        [JsonProperty]
        public int LastContentWordLevel
        {
            get => _lastContentWordLevel;
            set
            {
                _lastContentWordLevel = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
