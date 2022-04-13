using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClearDashboard.DataAccessLayer.Models
{
    public class VerseLink
    {
        public int Id { get; set; }
        public int VerseId { get; set; }
        public int ParallelVersesLinkId { get; set; }
        public virtual Verse Verse { get ;set; }
        public virtual ParallelVersesLink  ParallelVersesLink { get ; set; }
        public bool IsSource { get; set; } 
    }
}
