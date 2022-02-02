using System.Collections.ObjectModel;
using System.Windows.Documents;
using System.Windows.Media;
using ClearDashboard.Wpf.Helpers;

namespace ClearDashboard.Wpf.ViewModels
{
    public class ColorStylesViewModel: BindableBase
    {

        ObservableCollection<Inline> _inlinesText = new ObservableCollection<Inline>();
        public ObservableCollection<Inline> InlinesText
        {
            get { return _inlinesText; }
            set
            {
                _inlinesText = value;
                OnPropertyChanged("InlinesText");
            }
        }

        public ColorStylesViewModel()
        {
            _inlinesText.Add(new Run("First text")
            {
                Background = Brushes.BurlyWood,
                Foreground = Brushes.Red
            });
            _inlinesText.Add(new Run("Second text")
            {
                Background = Brushes.Cyan,
                Foreground = Brushes.Green
            });
            _inlinesText.Add(new Run("Third text")
            {
                Background = Brushes.Cornsilk,
                Foreground = Brushes.DarkBlue
            });
        }
    }
}
