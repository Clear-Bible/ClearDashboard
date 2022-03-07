using MvvmHelpers;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Windows.Documents;
using System.Windows.Media;
using Caliburn.Micro;

namespace ClearDashboard.Wpf.ViewModels
{
    public class ColorStylesViewModel: PropertyChangedBase
    {

        private ObservableCollection<Inline> _InlinesText;
        [JsonProperty]
        public ObservableCollection<Inline> InlinesText
        {
            get => _InlinesText;
            set
            {
                _InlinesText = value;
                NotifyOfPropertyChange(() => InlinesText);
            }
        }

        public ColorStylesViewModel()
        {
            _InlinesText.Add(new Run("First text")
            {
                Background = Brushes.BurlyWood,
                Foreground = Brushes.Red
            });
            _InlinesText.Add(new Run("Second text")
            {
                Background = Brushes.Cyan,
                Foreground = Brushes.Green
            });
            _InlinesText.Add(new Run("Third text")
            {
                Background = Brushes.Cornsilk,
                Foreground = Brushes.DarkBlue
            });
        }
    }
}
