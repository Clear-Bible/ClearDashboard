using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Documents;
using ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.DAL.ViewModels
{
    public class VerseViewModel : ViewModelBase<Verse>
    {
        public VerseViewModel() : base()
        {
            
        }

        public VerseViewModel(Verse entity): base(entity)
        {

        }

        public int Id
        {
            get => Entity.Id;
            set
            {
                if (Entity != null)
                {
                    Entity.Id = value;
                }
                NotifyOfPropertyChange(nameof(Id));
            }
        }

        public string? VerseNumber
        {
            get => Entity?.VerseNumber;
            set
            {
                if (Entity != null)
                {
                    Entity.VerseNumber = value;
                }
                NotifyOfPropertyChange(nameof(VerseNumber));
            }
        }

        public string? SilBookNumber
        {
            get => Entity?.SilBookNumber;
            set
            {
                if (Entity != null)
                {
                    Entity.SilBookNumber = value;
                }
                NotifyOfPropertyChange(nameof(SilBookNumber));
            }
        }

        public string? ChapterNumber
        {
            get => Entity?.ChapterNumber;
            set
            {
                if (Entity != null)
                {
                    Entity.ChapterNumber = value;
                }
                NotifyOfPropertyChange(nameof(ChapterNumber));
            }
        }

        public string? VerseText
        {
            get => Entity?.VerseText;
            set
            {
                if (Entity != null)
                {
                    Entity.VerseText = value;
                }
                NotifyOfPropertyChange(nameof(VerseText));
            }
        }

        public DateTimeOffset Modified
        {
            get => Entity.Modified;
            set
            {
                if (Entity != null)
                {
                    Entity.Modified = value;
                }
                NotifyOfPropertyChange(nameof(Modified));
            }
        }

        public int? CorpusId
        {
            get => Entity?.CorpusId;
            set
            {
                if (Entity != null)
                {
                    Entity.CorpusId = value;
                }
                NotifyOfPropertyChange(nameof(CorpusId));
            }
        }

        public virtual Corpus? Corpus
        {
            get => Entity?.Corpus;
            set
            {
                if (Entity != null)
                {
                    Entity.Corpus = value;
                }
                NotifyOfPropertyChange(nameof(Corpus));
            }
        }

        public virtual Token? Token
        {
            get => Entity?.Token;
            set
            {
                if (Entity != null)
                {
                    Entity.Token = value;
                }
                NotifyOfPropertyChange(nameof(Token));
            }
        }

        public virtual ICollection<ParallelVersesLink> ParallelVersesLinks
        {
            get => Entity?.ParallelVersesLinks;
            set
            {
                if (Entity != null)
                {
                    Entity.ParallelVersesLinks = value;
                }
                NotifyOfPropertyChange(nameof(ParallelVersesLinks));
            }
        }


        
        public string? VerseBBCCCVVV
        {
            get => Entity?.VerseBBCCCVVV;
            set
            {
                if (Entity != null)
                {
                    Entity.VerseBBCCCVVV = value;
                }
                NotifyOfPropertyChange(nameof(VerseBBCCCVVV));
            }
        }

        //public string? BookStr
        //{
        //    get => Entity?.BookStr;
        //    set
        //    {
        //        if (Entity != null)
        //        {
        //            Entity.BookStr = value;
        //        }
        //        NotifyOfPropertyChange(nameof(BookStr));
        //    }
        //}


        public string? BookStr => Entity?.BookStr;

        public string? ChapterStr => Entity?.ChapterStr;

        public string? VerseStr => Entity?.VerseStr;

        public string VerseId { get; set; } = string.Empty;

        public bool Found { get; set; }

        public ObservableCollection<Inline> Inlines { get; set; } = new ObservableCollection<Inline>();

        public VerseViewModel SetVerseFromBBBCCCVVV(string bbbcccvvv)
        {
            Entity.SetVerseFromBBBCCCVVV(bbbcccvvv);
            return this;
        }
    }
}
