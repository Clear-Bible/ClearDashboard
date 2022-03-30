using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;

namespace ClearDashboard.Common.Models
{
    public class DashboardProject : INotifyPropertyChanged
    {
        public string DirectoryPath { get; set; }
        public string ClearEngineDirectoryPath => Path.Combine(DirectoryPath, "ClearEngine");
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

                var fi = new FileInfo(_fullFilePath);
                ProjectPath = fi.DirectoryName;

                OnPropertyChanged();
            }
        }


        /// <summary>
        /// the target project
        /// </summary>
        private ParatextProject _targetProject;
        [JsonProperty]
        public ParatextProject TargetProject
        {
            get => _targetProject;
            set
            {
                _targetProject = value;
                this.BaseTargetName = _targetProject.Name;
                this.BaseTargetFullName = _targetProject.FullName;
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
        private List<ParatextProject> _btProjects;
        [JsonProperty]
        public List<ParatextProject> BTProjects
        {
            get => _btProjects;
            set => _btProjects = value;
        }

        /// <summary>
        /// The Paratext UserID of the creator
        /// </summary>
        private string _paratextUser;
        [JsonProperty]
        public string ParatextUser
        {
            get => _paratextUser;
            set => _paratextUser = value;
        }

        /// <summary>
        /// Date that this project was created
        /// </summary>
        private DateTime _creationDate;
        [JsonProperty]
        public DateTime CreationDate
        {
            get => _creationDate;
            set => _creationDate = value;
        }

        /// <summary>
        /// The Dashboard Project Name
        /// </summary>
        private string _name;
        [JsonProperty]
        public string Name
        {
            get => _name;
            set => _name = value;
        }

        /// <summary>
        /// The Dashboard Project Name
        /// </summary>
        private string _baseTargetName;
        [JsonProperty]
        public string BaseTargetName
        {
            get => _baseTargetName;
            set => _baseTargetName = value;
        }

        /// <summary>
        /// The Dashboard Project FullName
        /// </summary>
        private string _baseTargetFullName;
        [JsonProperty]
        public string BaseTargetFullName
        {
            get => _baseTargetFullName;
            set => _baseTargetFullName = value;
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
