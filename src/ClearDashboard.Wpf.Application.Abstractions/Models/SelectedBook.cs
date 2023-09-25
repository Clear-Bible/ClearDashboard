using Caliburn.Micro;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

namespace ClearDashboard.Wpf.Application.Models
{
    public class SelectedBook : PropertyChangedBase, INotifyPropertyChanged
    {
        public enum BookColors
        {
            Pentateuch = 0,
            Historical = 1,
            Wisdom = 2,
            Prophets = 3,
            Gospels = 5,
            Acts = 6,
            Epistles = 7,
            Revelation = 8,

        }

        public string? BookName { get; set; }

        public FontWeight FontWeight { get; set; }

        public bool IsImported { get; set; }

        public bool IsSelected
        {
            get => _isSelected;
            set => Set(ref _isSelected, value);
        }

        public bool IsEnabled { get; set; }

        public bool HasUsfmError { get; set; }

        public string Abbreviation { get; set; }

        public Brush BackColor { get; set; }

        public float ProgressNum { get; set; }


        private BookColors _colorText;
        private bool _isSelected;

        public BookColors ColorText
        {
            get => _colorText;
            set
            {
                _colorText = value;

                switch (value)
                {
                    // OT
                    case BookColors.Pentateuch:
                        BookColor = (SolidColorBrush?)System.Windows.Application.Current.FindResource("PrimaryHueDarkBrush");
                        break;
                    case BookColors.Historical:
                        BookColor = (SolidColorBrush?)System.Windows.Application.Current.FindResource("SecondaryHueDarkBrush");
                        break;
                    case BookColors.Wisdom:
                        BookColor = (SolidColorBrush?)System.Windows.Application.Current.FindResource("PrimaryHueDarkBrush");
                        break;
                    case BookColors.Prophets:
                        BookColor =(SolidColorBrush?)System.Windows.Application.Current.FindResource("SecondaryHueDarkBrush");
                        break;

                    // NT
                    case BookColors.Gospels:
                        BookColor = (SolidColorBrush?)System.Windows.Application.Current.FindResource("PrimaryHueDarkBrush");
                        break;
                    case BookColors.Acts:
                        BookColor = (SolidColorBrush?)System.Windows.Application.Current.FindResource("SecondaryHueDarkBrush");
                        break;
                    case BookColors.Epistles:
                        BookColor = (SolidColorBrush?)System.Windows.Application.Current.FindResource("PrimaryHueDarkBrush");
                        break;
                    case BookColors.Revelation:
                        BookColor = (SolidColorBrush?)System.Windows.Application.Current.FindResource("SecondaryHueDarkBrush");
                        break;

                }

            }
        }


        public SolidColorBrush? BookColor { get; set; }

        public string? BookNum { get; set; }

        public bool IsOldTestament { get; set; }

    }
}
