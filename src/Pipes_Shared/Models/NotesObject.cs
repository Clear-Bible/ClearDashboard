using MvvmHelpers;
using System.Collections.Generic;

namespace ClearDashboard.Pipes_Shared.Models
{
    internal class NotesObject : ObservableObject
    {
        private Anchor _Anchor;
        public Anchor Anchor
        {
            get => _Anchor;
            set { SetProperty(ref _Anchor, value); }
        }

        private List<Comment> comments;
        public List<Comment> Comments
        {
            get => comments;
            set { SetProperty(ref comments, value); }
        }

        private bool _isRead;
        public bool IsRead
        {
            get => _isRead;
            set { SetProperty(ref _isRead, value); }
        }

        private bool _IsResolved;
        public bool IsResolved
        {
            get => _IsResolved;
            set { SetProperty(ref _IsResolved, value); }
        }

        private AssignedUser _AssignedUser;
        public AssignedUser AssignedUser
        {
            get => _AssignedUser;
            set { SetProperty(ref _AssignedUser, value); }
        }

        private object _ReplyToUser;
        public object ReplyToUser
        {
            get => _ReplyToUser;
            set { SetProperty(ref _ReplyToUser, value); }
        }




    }
}
