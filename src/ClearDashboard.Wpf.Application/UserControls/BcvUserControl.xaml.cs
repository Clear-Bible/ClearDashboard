using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ClearDashboard.DAL.ViewModels;

namespace ClearDashboard.Wpf.Application.UserControls
{
    /// <summary>
    /// Interaction logic for BookChapterVerse.xaml
    /// </summary>
    public partial class BcvUserControl : UserControl
    {
        #region Member Variables


        #endregion

        #region Observable Properties

        public static readonly DependencyProperty _paratextSync =
            DependencyProperty.Register("ParatextSync", typeof(bool), typeof(BcvUserControl),
                new PropertyMetadata(true));
        public bool ParatextSync
        {
            get => (bool)GetValue(_paratextSync);
            set => SetValue(_paratextSync, value);
        }


        public static readonly DependencyProperty _isRtl =
            DependencyProperty.Register("IsRtl", typeof(bool), typeof(BcvUserControl),
            new PropertyMetadata(false));

        public bool IsRtl
        {
            get => (bool)GetValue(_isRtl);
            set => SetValue(_isRtl, value);
        }


        public static readonly DependencyProperty _currentBcv =
            DependencyProperty.Register("CurrentBcv", typeof(BookChapterVerseViewModel), typeof(BcvUserControl),
                new PropertyMetadata(new BookChapterVerseViewModel()));

        public BookChapterVerseViewModel CurrentBcv
        {
            get => (BookChapterVerseViewModel)GetValue(_currentBcv);
            set => SetValue(_currentBcv, value);
        }


        //public static readonly DependencyProperty _bookNames =
        //    DependencyProperty.Register("BookNames", typeof(ObservableCollection<string>), typeof(BcvUserControl),
        //    new PropertyMetadata(new ObservableCollection<string>()));

        //public ObservableCollection<string> BookNames
        //{
        //    get => (ObservableCollection<string>)GetValue(_bookNames);
        //    set => SetValue(_bookNames, value);
        //}

        #endregion

        #region Constructor
        public BcvUserControl()
        {
            InitializeComponent();
            LayoutRoot.DataContext = this;

            // Rotate the arrows
            RotateTransform rotate = new()
            {
                Angle = 180
            };

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
            BtnBookLeft.IsEnabled = true;
            BtnBookRight.IsEnabled = true;

            BtnChapterLeft.IsEnabled = true;
            BtnChapterRight.IsEnabled = true;

            BtnVerseLeft.IsEnabled = true;
            BtnVerseRight.IsEnabled = true;
        }

        #endregion

        #region Methods

        private void BookUpArrow_Click(object sender, RoutedEventArgs e)
        {
            if (CboBook.SelectedIndex > 0)
            {
                CboBook.SelectedIndex -= 1;
            }
        }

        private void BookDownArrow_Click(object sender, RoutedEventArgs e)
        {
            CboBook.SelectedIndex += 1;
        }

        private void ChapterUpArrow_Click(object sender, RoutedEventArgs e)
        {
            if (CboChapter.SelectedIndex > 0)
            {
                CboChapter.SelectedIndex -= 1;
            }
            else // Switch to the previous book.
            {
                BookUpArrow_Click(null, null);
                CboChapter.SelectedIndex = CboChapter.Items.Count - 1;
            }
        }

        private void ChapterDownArrow_Click(object sender, RoutedEventArgs e)
        {
            if (CboChapter.SelectedIndex < CboChapter.Items.Count - 1)
            {
                CboChapter.SelectedIndex += 1;
            }
            else // Switch to the next book.
            {
                BookDownArrow_Click(null, null);
                CboChapter.SelectedIndex = 0;
            }
        }

        private void VerseUpArrow_Click(object sender, RoutedEventArgs e)
        {
            if (CboVerse.SelectedIndex > 0)
            {
                CboVerse.SelectedIndex -= 1;
            }
            else // Switch to the previous chapter.
            {
                ChapterUpArrow_Click(null, null);
                CboVerse.SelectedIndex = CboVerse.Items.Count - 1;
            }
        }

        private void VerseDownArrow_Click(object sender, RoutedEventArgs e)
        {
            if (CboVerse.SelectedIndex < CboVerse.Items.Count - 1)
            {
                CboVerse.SelectedIndex += 1;
            }
            else // Switch to the next chapter.
            {
                ChapterDownArrow_Click(null, null);
                CboVerse.SelectedIndex = 0;
            }
        }

        #endregion
    }
}
