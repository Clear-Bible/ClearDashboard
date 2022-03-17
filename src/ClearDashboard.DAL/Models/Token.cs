using System;
using System.Collections.Generic;

namespace ClearDashboard.DataAccessLayer.Models
{
    public partial class Token
    {
        public Token()
        {
            InterlinearNotes = new HashSet<InterlinearNote>();
        }

        public long Id { get; set; }
        public long WordId { get; set; }
        public long PartId { get; set; }
        public long VerseId { get; set; }
        public string Text { get; set; }
        public string FirstLetter { get; set; }

        public virtual Alignment Token1 { get; set; }
        public virtual Alignment TokenNavigation { get; set; }
        public virtual Adornment Adornment { get; set; }
        public virtual Verse Verse { get; set; }
        public virtual ICollection<InterlinearNote> InterlinearNotes { get; set; }
    }
}
