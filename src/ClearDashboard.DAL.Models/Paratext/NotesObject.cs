using System.Collections.Generic;
using ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.DataAccessLayer.Models
{
    public class NotesObject 
    {
       
        public Anchor Anchor { get; set; }

        public List<Comment> Comments { get; set; } = new List<Comment>();

        public bool IsRead { get; set; }

        public bool IsResolved { get; set; }

        
        public AssignedUser AssignedUser { get; set; }

        public object ReplyToUser { get; set; }

    }
}
