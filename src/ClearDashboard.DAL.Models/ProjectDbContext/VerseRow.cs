﻿using SIL.Scripture;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClearDashboard.DataAccessLayer.Models
{
    public class VerseRow : SynchronizableTimestampedEntity
    {
        public VerseRow() : base()
        {
            // ReSharper disable VirtualMemberCallInConstructor
            TokenComponents = new HashSet<TokenComponent>();
            // ReSharper restore VirtualMemberCallInConstructor
        }

        public string? BookChapterVerse { get; set; }

        public string? OriginalText { get; set; }
        public bool IsSentenceStart { get; set; }
        public bool IsInRange { get; set; }
        public bool IsRangeStart { get; set; }
        public bool IsEmpty { get; set; }

        [ForeignKey(nameof(TokenizedCorpusId))]
        public Guid TokenizedCorpusId { get; set; }
        public virtual TokenizedCorpus? TokenizedCorpus { get; set; }
        public virtual ICollection<TokenComponent> TokenComponents { get; set; }

        [NotMapped]
        public IEnumerable<Token> Tokens => TokenComponents.Where(tc => tc.GetType() == typeof(Token)).Cast<Token>();

        [NotMapped]
        public IEnumerable<TokenComposite> TokenComposites => TokenComponents.Where(tc => tc.GetType() == typeof(TokenComposite)).Cast<TokenComposite>();
        public DateTimeOffset? Modified { get; set; }
    }
}