using System;
using System.Collections.Generic;

namespace ClearDashboard.DataAccessLayer.Models
{
    public partial class InterlinearNote
    {
        public long Id { get; set; }
        public long? TokenId { get; set; }
        public string Note { get; set; }
        public long UserId { get; set; }
        public byte[] CreationDate { get; set; }

        public virtual Token Token { get; set; }
        public virtual User User { get; set; }
    }
}
