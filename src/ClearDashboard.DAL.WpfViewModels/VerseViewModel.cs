using System.Collections.ObjectModel;
using System.Windows.Documents;
using ClearDashboard.DAL.ViewModels;
using ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.DAL.Wpf.ViewModels
{
    public class VerseViewModel : ViewModelBase<VerseObject>
    {
        public VerseViewModel() : base()
        {
            
        }

        public VerseViewModel(VerseObject entity): base(entity)
        {

        }

        public Guid? Id
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

        public int? BookNumber
        {
            get => Entity?.BookNumber;
            set
            {
                if (Entity != null)
                {
                    Entity.BookNumber = value;
                }
                NotifyOfPropertyChange(nameof(BookNumber));
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

        public string? RichVerseText
        {
            get => Entity?.RichVerseText;
            set
            {
                if (Entity != null)
                {
                    Entity.RichVerseText = value;
                }
                NotifyOfPropertyChange(nameof(RichVerseText));
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


        // TODO:  This needs to be removed and the code that refers to it refacotred.
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


        public string? BookStr => Entity?.SilBookAbbreviation;

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
