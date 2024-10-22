﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;

namespace ClearDashboard.Common.Models
{
    public class DashboardProject : INotifyPropertyChanged
    {
        public string DirectoryPath { get; set; }
       //TODO:  fix this to use the correct CLear.Engine directory path
       // public string ClearEngineDirectoryPath => Path.Combine(TargetProject.DirectoryPath, "ClearEngine");
        public bool HasJsonProjectName { get; set; } = false;

        public DashboardProject()
        {
            LanguageOfWiderCommunicationProjects = new List<ParatextProject>();
            BackTranslationProjects = new List<ParatextProject>();
            InterlinearizerProject = null;
        }

        private string _projectName;
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

        private ParatextProject _interlinearizerProject;
        public ParatextProject InterlinearizerProject
        {
            get => _interlinearizerProject;
            set => _interlinearizerProject = value;
        }


        /// <summary>
        /// list of LWC projects
        /// </summary>
        private List<ParatextProject> _languageOfWiderCommunicationProjects;
        public List<ParatextProject> LanguageOfWiderCommunicationProjects
        {
            get => _languageOfWiderCommunicationProjects;
            set => _languageOfWiderCommunicationProjects = value;
        }

        /// <summary>
        /// List of Back Translation projects
        /// </summary>
        private List<ParatextProject> _backTranslationProjects;
        public List<ParatextProject> BackTranslationProjects
        {
            get => _backTranslationProjects;
            set => _backTranslationProjects = value;
        }

        /// <summary>
        /// The Paratext UserID of the creator
        /// </summary>
        private string _paratextUser;
        public string ParatextUser
        {
            get => _paratextUser;
            set => _paratextUser = value;
        }

        /// <summary>
        /// Date that this project was created
        /// </summary>
        private DateTime _creationDate;
        public DateTime CreationDate
        {
            get => _creationDate;
            set => _creationDate = value;
        }

        /// <summary>
        /// The Dashboard Project Name
        /// </summary>
        private string _name;
        public string Name
        {
            get => _name;
            set => _name = value;
        }

        /// <summary>
        /// The Dashboard Project Name
        /// </summary>
        private string _baseTargetName;
        public string BaseTargetName
        {
            get => _baseTargetName;
            set => _baseTargetName = value;
        }

        /// <summary>
        /// The Dashboard Project FullName
        /// </summary>
        private string _baseTargetFullName;
        public string BaseTargetFullName
        {
            get => _baseTargetFullName;
            set => _baseTargetFullName = value;
        }

        private string _shortFilePath;
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

        public bool ValidateProjectData()
        {
            if (ProjectName is "" or null)
            {
                return false;
            }

            // check to see if we have at least a target project
            return TargetProject is not null;
        }
    }
}
