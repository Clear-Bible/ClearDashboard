using System.ComponentModel.DataAnnotations.Schema;

namespace ClearDashboard.DataAccessLayer.Models
{
    public  class Token : TokenComponent
    {
        public Token() : base()
        {
            // ReSharper disable VirtualMemberCallInConstructor
            TokenComposites = new HashSet<TokenComposite>();
            TokenCompositeTokenAssociations = new HashSet<TokenCompositeTokenAssociation>();
            // ReSharper restore VirtualMemberCallInConstructor
        }

        public int BookNumber { get; set; }
        public int ChapterNumber { get; set; }
        public int VerseNumber { get; set; }
        public int WordNumber { get; set; }
        public int SubwordNumber { get; set; }


        public virtual Adornment? Adornment { get; set; }
        public ICollection<TokenComposite> TokenComposites { get; set; }
        public ICollection<TokenCompositeTokenAssociation> TokenCompositeTokenAssociations { get; set; }
    }
}
