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
        /// <summary>
        /// Non-null if there are have been any post-tokenization changes
        /// or if any token in the same 'word' (same BCVW) was created manually, 
        /// e.g. via token splitting.  If non-null, the collaboration system 
        /// will include the token in its serialized data transfer and subsequent 
        /// merging.  When merging, this is used for source-target system token 
        /// matching (as a system-neutral Id).  
        /// </summary>
        public string? OriginTokenLocation { get; set; }

        public virtual Adornment? Adornment { get; set; }
        public ICollection<TokenComposite> TokenComposites { get; set; }
        public ICollection<TokenCompositeTokenAssociation> TokenCompositeTokenAssociations { get; set; }
    }
}
