using MvvmHelpers;
using System;
using System.Collections.Generic;

namespace ClearDashboard.Pipes_Shared.Models
{
    public class Comment : ObservableObject
    {
        private List<Content> _contents;
        public List<Content> Contents
        {
            get => _contents;
            set => SetProperty(ref _contents, value);
        }

        private Author _author;
        public Author Author
        {
            get => _author;
            set => SetProperty(ref _author, value);
        }

        private DateTimeOffset _created;
        public DateTimeOffset Created
        {
            get => _created;
            set => SetProperty(ref _created, value);
        }

        private SelectedLanguage _selectedLanguage;
        public SelectedLanguage SelectedLanguage
        {
            get => _selectedLanguage;
            set => SetProperty(ref _selectedLanguage, value);
        }

        private AssignedUser _assignedUser;
        public AssignedUser AssignedUser
        {
            get => _assignedUser;
            set => SetProperty(ref _assignedUser, value);
        }
    }
}