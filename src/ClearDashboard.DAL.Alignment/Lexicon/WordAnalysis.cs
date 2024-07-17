using System.Collections.ObjectModel;
using ClearBible.Engine.Utils;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Exceptions;
using ClearDashboard.DAL.Alignment.Features;
using ClearDashboard.DAL.Alignment.Features.Lexicon;
using MediatR;

namespace ClearDashboard.DAL.Alignment.Lexicon
{
    public class WordAnalysis : IEquatable<WordAnalysis>
    {
        public WordAnalysisId WordAnalysisId
        {
            get;
#if DEBUG
            set;
#else 
            // RELEASE MODIFIED
            //private set;
            set;
#endif
        }

        private string language_;
        public string Language 
        {
            get => language_;
            set
            {
                language_ = value;
                IsDirty = true;
            }
        }

        private string word_;
        public string Word 
        { 
            get => word_;
            set
            {
                word_ = value;
                IsDirty = true;
            }
        }

#if DEBUG
        private ObservableCollection<Lexeme> lexemes_;
#else
        // RELEASE MODIFIED
        //private readonly ObservableCollection<Lexeme> lexemes_;
        private ObservableCollection<Lexeme> lexemes_;
#endif

		public ObservableCollection<Lexeme> Lexemes
		{
            get { return lexemes_; }
#if DEBUG
            set { lexemes_ = value; }
#else
            // RELEASE MODIFIED
            //set { lexemes_ = value; }
            set { lexemes_ = value; }
#endif
        }

        public bool IsDirty { get; internal set; } = false;
        public bool IsInDatabase { get => WordAnalysisId.Created is not null; }

        public WordAnalysis()
        {
			WordAnalysisId = WordAnalysisId.Create(Guid.NewGuid());

            word_ = string.Empty;
            language_ = string.Empty;
			lexemes_ = new ObservableCollection<Lexeme>();
        }
        internal WordAnalysis(WordAnalysisId wordAnalysisId, string language, string word, ICollection<Lexeme> lexemes)
        {
            WordAnalysisId = wordAnalysisId;

            language_ = language;
            word_ = word;

            lexemes_ = new ObservableCollection<Lexeme>(lexemes.DistinctBy(s => s.LexemeId));
        }

        public override bool Equals(object? obj) => Equals(obj as WordAnalysis);
        public bool Equals(WordAnalysis? other)
        {
            if (other is null) return false;
            if (!WordAnalysisId.Id.Equals(other.WordAnalysisId.Id)) return false;

            return true;
        }
        public override int GetHashCode() => WordAnalysisId.Id.GetHashCode();
        public static bool operator ==(WordAnalysis? e1, WordAnalysis? e2) => Equals(e1, e2);
        public static bool operator !=(WordAnalysis? e1, WordAnalysis? e2) => !(e1 == e2);
    }
}
