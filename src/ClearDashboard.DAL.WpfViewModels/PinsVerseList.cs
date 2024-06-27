using ClearDashboard.DAL.Wpf.ViewModels;

namespace ClearDashboard.DAL.Wpf.ViewModels
{
    public class PinsVerseListViewModel: VerseViewModel
    {
        //public string? BBBCCCVVV { get; set; }
        public string? VerseIdShort { get; set; }
       // public string? VerseText { get; set; }
        public string? BackTranslation { get; set; } = string.Empty;

        private bool _showBackTranslation = false;
        public bool ShowBackTranslation
        {
            get => _showBackTranslation;

            set
            {
                _showBackTranslation = value;

                NotifyOfPropertyChange(() => ShowBackTranslation);
            }
        }
        //public bool Found { get; set; }
        //public ObservableCollection<Inline> Inlines { get; set; } = new ObservableCollection<Inline>();
    }
}
