using ClearDashboard.Common.Models;
using ClearDashboard.Wpf.ViewModels;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ClearDashboard.Wpf.UserControls
{
    /// <summary>
    /// Interaction logic for BookChapterVerse.xaml
    /// </summary>
    public partial class BcvUserControl : UserControl
    {
        public static readonly DependencyProperty _isRtl =
            DependencyProperty.Register("IsRtl", typeof(bool), typeof(BcvUserControl),
            new PropertyMetadata(false));

        public bool IsRtl
        {
            get => (bool)GetValue(_isRtl);
            set => SetValue(_isRtl, value);
        }

        public static readonly DependencyProperty _currentBCV =
            DependencyProperty.Register("CurrentBCV", typeof(BookChapterVerse), typeof(BcvUserControl),
                new PropertyMetadata(new BookChapterVerse()));

        public BookChapterVerse CurrentBCV
        {
            get
            {
                return (BookChapterVerse)GetValue(_currentBCV);
            }
            set
            {
                SetValue(_currentBCV, value);
            }
        }

        public static readonly DependencyProperty _bookNames =
            DependencyProperty.Register("BookNames", typeof(ObservableCollection<string>), typeof(BcvUserControl),
            new PropertyMetadata(new ObservableCollection<string>()));

        public ObservableCollection<string> BookNames
        {
            get => (ObservableCollection<string>)GetValue(_bookNames);
            set => SetValue(_bookNames, value);
        }

        public static readonly DependencyProperty _chapNums =
            DependencyProperty.Register("ChapNums", typeof(ObservableCollection<int>), typeof(BcvUserControl),
            new PropertyMetadata(new ObservableCollection<int>()));

        public ObservableCollection<int> ChapNums
        {
            get => (ObservableCollection<int>)GetValue(_chapNums);
            set => SetValue(_chapNums, value);
        }

        public static readonly DependencyProperty _verseNums =
            DependencyProperty.Register("VerseNums", typeof(ObservableCollection<int>), typeof(BcvUserControl),
            new PropertyMetadata(new ObservableCollection<int>()));

        public ObservableCollection<int> VerseNums
        {
            get => (ObservableCollection<int>)GetValue(_verseNums);
            set => SetValue(_verseNums, value);
        }


        public BcvUserControl()
        {
            InitializeComponent();
            LayoutRoot.DataContext = this;

            // Rotate the arrows
            RotateTransform rotate = new();
            rotate.Angle = 180;

            if (IsRtl)
            {
                NextBookArrow.LayoutTransform = rotate;
                NextChapterArrow.LayoutTransform = rotate;
                NextVerseArrow.LayoutTransform = rotate;
            }
            else
            {
                PreviousBookArrow.LayoutTransform = rotate;
                PreviousChapterArrow.LayoutTransform = rotate;
                PreviousVerseArrow.LayoutTransform = rotate;
            }

            // Why are these not enabled by default? I did not find an instance of them not enable in the project.
            btnBookLeft.IsEnabled = true;
            btnBookRight.IsEnabled = true;

            btnChapterLeft.IsEnabled = true;
            btnChapterRight.IsEnabled = true;

            btnVerseLeft.IsEnabled = true;
            btnVerseRight.IsEnabled = true;


        }

        private void SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.RoutedEvent.Name == "SelectionChanged" && ((FrameworkElement)(UIElement)e.OriginalSource).IsEnabled)
            {
                if (((FrameworkElement)e.OriginalSource).Name == "cboBook")
                {
                    cboChapter.SelectedIndex = 0;
                }
                else if (((FrameworkElement)e.OriginalSource).Name == "cboChapter")
                {
                    cboVerse.SelectedIndex = 0;
                }
            }
        }

        private void BookUpArrow_Click(object sender, RoutedEventArgs e)
        {
            if (cboBook.SelectedIndex > 0)
            {
                cboBook.SelectedIndex -= 1;
            }
        }

        private void BookDownArrow_Click(object sender, RoutedEventArgs e)
        {
            cboBook.SelectedIndex += 1;
        }

        private void ChapterUpArrow_Click(object sender, RoutedEventArgs e)
        {
            if (cboChapter.SelectedIndex > 0)
            {
                cboChapter.SelectedIndex -= 1;
            }
            else // Switch to the previous book.
            {
                BookUpArrow_Click(null, null);
                cboChapter.SelectedIndex = cboChapter.Items.Count - 1;
            }
        }

        private void ChapterDownArrow_Click(object sender, RoutedEventArgs e)
        {
            if (cboChapter.SelectedIndex < cboChapter.Items.Count - 1)
            {
                cboChapter.SelectedIndex += 1;
            }
            else // Switch to the next book.
            {
                BookDownArrow_Click(null, null);
                cboChapter.SelectedIndex = 0;
            }
        }

        private void VerseUpArrow_Click(object sender, RoutedEventArgs e)
        {
            if (cboVerse.SelectedIndex > 0)
            {
                cboVerse.SelectedIndex -= 1;
            }
            else // Switch to the previous chapter.
            {
                ChapterUpArrow_Click(null, null);
                cboVerse.SelectedIndex = cboVerse.Items.Count - 1;
            }
        }

        private void VerseDownArrow_Click(object sender, RoutedEventArgs e)
        {
            if (cboVerse.SelectedIndex < cboVerse.Items.Count - 1)
            {
                cboVerse.SelectedIndex += 1;
            }
            else // Switch to the next chapter.
            {
                ChapterDownArrow_Click(null, null);
                cboVerse.SelectedIndex = 0;
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            INotifyPropertyChanged viewModel = (INotifyPropertyChanged)this.DataContext;
            viewModel.PropertyChanged += (sender, args) => {
                if (args.PropertyName.Equals("CurrentBcv"))



                    return;
                // execute code here.
            };

            //if (DataContext is WorkSpaceViewModel vm)
            //{
            //    CurrentBCV = vm.CurrentBcv;
            //}
            
        }
    }
}
