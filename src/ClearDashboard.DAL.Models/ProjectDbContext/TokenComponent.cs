using System.ComponentModel.DataAnnotations.Schema;

namespace ClearDashboard.DataAccessLayer.Models
{
    public abstract class TokenComponent : IdentifiableEntity
    {
        public TokenComponent() : base()
        {
            // ReSharper disable VirtualMemberCallInConstructor
            TokenVerseAssociations = new HashSet<TokenVerseAssociation>();
            SourceAlignments = new HashSet<Alignment>();
            TargetAlignments = new HashSet<Alignment>();
            Translations = new HashSet<Translation>();
            // ReSharper restore VirtualMemberCallInConstructor
        }

        public string? EngineTokenId { get; set; }
        public string? TrainingText { get; set; }
        public string? SurfaceText { get; set; }

        /// <summary>
        /// Optional type analogous to Lexicon_Lexeme.Type (lemma, suffix, etc.)
        /// </summary>
        public string? Type { get; set; }

        /// <summary>
        /// If the token is part of a circumfix group, this is the group name.
        /// </summary>
        public string? CircumfixGroup { get; set; }

		/// <summary>
		/// The grammar type associated to this token.
		/// </summary>
		[ForeignKey(nameof(GrammarId))]
		public Guid? GrammarId { get; set; }
		public virtual Grammar? Grammar { get; set; }

        public string? ExtendedProperties { get; set; }

        [ForeignKey(nameof(VerseRowId))]
        public Guid? VerseRowId { get; set; }
        public virtual VerseRow? VerseRow { get; set; }

        [ForeignKey(nameof(TokenizedCorpusId))]
        public Guid TokenizedCorpusId { get; set; }
        public virtual TokenizedCorpus? TokenizedCorpus { get; set; }

        public virtual ICollection<TokenVerseAssociation> TokenVerseAssociations { get; set; }
        public virtual ICollection<Alignment> SourceAlignments { get; set; }
        public virtual ICollection<Alignment> TargetAlignments { get; set; }
        public virtual ICollection<Translation> Translations { get; set; }

        public DateTimeOffset? Deleted { get; set; }

        /// <summary>
        /// Metadata for the token component - stored as a JSON blob in the database.
        /// </summary>
        public virtual List<Metadatum> Metadata { get; set; } = new();
    }
}
