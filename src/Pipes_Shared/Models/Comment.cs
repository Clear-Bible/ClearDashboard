using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using MvvmHelpers;

namespace ClearDashboard.Pipes_Shared.Models
{
    public class Comment : ObservableObject
    {
        private List<Content> _Contents;
        [JsonProperty]
        public List<Content> Contents
        {
            get => _Contents;
            set { SetProperty(ref _Contents, value); }
        }

        private Author _Author;
        [JsonProperty]
        public Author Author
        {
            get => _Author;
            set { SetProperty(ref _Author, value); }
        }

        private DateTimeOffset _Created;
        [JsonProperty]
        public DateTimeOffset Created
        {
            get => _Created;
            set { SetProperty(ref _Created, value); }
        }

        private Language _Language;
        [JsonProperty]
        public Language Language
        {
            get => _Language;
            set { SetProperty(ref _Language, value); }
        }

        private AssignedUser _AssignedUser;
        [JsonProperty]
        public AssignedUser AssignedUser
        {
            get => _AssignedUser;
            set { SetProperty(ref _AssignedUser, value); }
        }
    }
}