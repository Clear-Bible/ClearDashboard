using MvvmHelpers;
using System.Collections.Generic;

namespace ClearDashboard.Pipes_Shared.Models
{
    internal class NotesObject : ObservableObject
    {
        private Anchor _anchor;
        public Anchor Anchor
        {
            get => _anchor;
            set => SetProperty(ref _anchor, value);
        }

        private List<Comment> _comments;
        public List<Comment> Comments
        {
            get => _comments;
            set => SetProperty(ref _comments, value);
        }

        private bool _isRead;
        public bool IsRead
        {
            get => _isRead;
            set => SetProperty(ref _isRead, value);
        }

        private bool _isResolved;
        public bool IsResolved
        {
            get => _isResolved;
            set => SetProperty(ref _isResolved, value);
        }

        private AssignedUser _assignedUser;
        public AssignedUser AssignedUser
        {
            get => _assignedUser;
            set => SetProperty(ref _assignedUser, value);
        }

        private object _replyToUser;
        public object ReplyToUser
        {
            get => _replyToUser;
            set => SetProperty(ref _replyToUser, value);
        }




    }
}
