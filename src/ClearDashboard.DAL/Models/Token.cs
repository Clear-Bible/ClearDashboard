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

        public int Id { get; set; }
        public int WordId { get; set; }
        public int PartId { get; set; }
        public int VerseId { get; set; }
        public string Text { get; set; }
        public string FirstLetter { get; set; }

        public virtual Alignment Token1 { get; set; }
        public virtual Alignment TokenNavigation { get; set; }
        public virtual Adornment Adornment { get; set; }
        public virtual Verse Verse { get; set; }
        public virtual ICollection<InterlinearNote> InterlinearNotes { get; set; }
    }
}
