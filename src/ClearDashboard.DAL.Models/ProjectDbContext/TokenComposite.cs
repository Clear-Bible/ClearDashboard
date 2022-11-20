using System.ComponentModel.DataAnnotations.Schema;

namespace ClearDashboard.DataAccessLayer.Models
{
    public  class TokenComposite : TokenComponent
    {
        public TokenComposite() : base()
        {
            // ReSharper disable VirtualMemberCallInConstructor
            Tokens = new HashSet<Token>();
            // ReSharper restore VirtualMemberCallInConstructor
        }
        public virtual ICollection<Token> Tokens { get; set; }

        public virtual Guid? ParallelCorpusId { get; set; }
        public virtual ParallelCorpus? ParallelCorpus { get; set; }
    }
}
