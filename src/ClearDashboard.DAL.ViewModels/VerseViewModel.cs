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

        public Guid Id
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

        public int? VerseNumber
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

        public int? SilBookNumber
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

        public int? ChapterNumber
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

     
        public Guid? CorpusId
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

        //TODO:  Update to reflect new schema changes

        //public virtual Token? Token
        //{
        //    get => Entity?.Token;
        //    set
        //    {
        //        if (Entity != null)
        //        {
        //            Entity.Token = value;
        //        }
        //        NotifyOfPropertyChange(nameof(Token));
        //    }
        //}

        //public virtual ICollection<ParallelVersesLink> ParallelVersesLinks
        //{
        //    get => Entity?.ParallelVersesLinks;
        //    set
        //    {
        //        if (Entity != null)
        //        {
        //            Entity.ParallelVersesLinks = value;
        //        }
        //        NotifyOfPropertyChange(nameof(ParallelVersesLinks));
        //    }
        //}


        
        public string? VerseBBCCCVVV
        {
            get => Entity?.VerseBBBCCCVVV;
            set
            {
                if (Entity != null)
                {
                    Entity.VerseBBBCCCVVV = value;
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

        public string? VerseString => Entity?.VerseString;

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
