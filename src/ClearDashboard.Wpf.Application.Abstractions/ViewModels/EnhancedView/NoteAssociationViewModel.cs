using Caliburn.Micro;
using ClearBible.Engine.Utils;
using ClearDashboard.DAL.Alignment;

namespace ClearDashboard.Wpf.Application.ViewModels.EnhancedView
{
    public class NoteAssociationViewModel : PropertyChangedBase
    {
        private string _description = string.Empty;
        private string _book;
        private string _chapter;
        private string _verse;
        private string _word;
        private string _part;

        public IId AssociatedEntityId { get; set; } = new EmptyEntityId();

        public string Description
        {
            get => _description;
            set
            {
                if (value == _description) return;
                _description = value;
                NotifyOfPropertyChange();
            }
        }

        public string Book
        {
            get => _book;
            set => Set(ref _book, value);
        }

        public string Chapter
        {
            get => _chapter;
            set => Set(ref _chapter, value);
        }

        public string Verse
        {
            get => _verse;
            set => Set(ref _verse, value);
        }

        public string Word
        {
            get => _word;
            set => Set(ref _word, value);
        }

        public string Part
        {
            get => _part;
            set => Set(ref _part, value);
        }
    }
}
