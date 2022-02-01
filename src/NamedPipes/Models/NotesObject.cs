using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ClearDashboard.NamedPipes.Models
{
    internal class NotesObject : BindableBase
    {
        private Anchor _Anchor;
        [JsonProperty]
        public Anchor Anchor
        {
            get => _Anchor;
            set { SetProperty(ref _Anchor, value); }
        }

        private List<Comment> comments;
        [JsonProperty]
        public List<Comment> Comments
        {
            get => comments;
            set { SetProperty(ref comments, value); }
        }

        private bool _isRead;
        [JsonProperty]
        public bool IsRead
        {
            get => _isRead;
            set { SetProperty(ref _isRead, value); }
        }

        private bool _IsResolved;
        [JsonProperty]
        public bool IsResolved
        {
            get => _IsResolved;
            set { SetProperty(ref _IsResolved, value); }
        }

        private AssignedUser _AssignedUser;
        [JsonProperty]
        public AssignedUser AssignedUser
        {
            get => _AssignedUser;
            set { SetProperty(ref _AssignedUser, value); }
        }

        private object _ReplyToUser;
        [JsonProperty]
        public object ReplyToUser
        {
            get => _ReplyToUser;
            set { SetProperty(ref _ReplyToUser, value); }
        }




    }
}
