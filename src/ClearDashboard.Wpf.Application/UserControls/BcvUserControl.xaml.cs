using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Caliburn.Micro;
using ClearDashboard.DAL.ViewModels;

namespace ClearDashboard.Wpf.Application.UserControls
{
    /// <summary>
    /// Interaction logic for BookChapterVerse.xaml
    /// </summary>
    public partial class BcvUserControl : UserControl, INotifyPropertyChanged
    {
        #region Member Variables


        #endregion

        #region Observable Properties

        public static readonly DependencyProperty ParatextSyncProperty =
            DependencyProperty.Register("ParatextSync", typeof(bool), typeof(BcvUserControl),
                new PropertyMetadata(true));
        public bool ParatextSync
        {
            get => (bool)GetValue(ParatextSyncProperty);
            set => SetValue(ParatextSyncProperty, value);
        }


        public static readonly DependencyProperty IsRtlProperty =
            DependencyProperty.Register("IsRtl", typeof(bool), typeof(BcvUserControl),
            new PropertyMetadata(false));

        public bool IsRtl
        {
            get => (bool)GetValue(IsRtlProperty);
            set => SetValue(IsRtlProperty, value);
        }


        public static readonly DependencyProperty CurrentBcvProperty =
            DependencyProperty.Register("CurrentBcv", typeof(BookChapterVerseViewModel), typeof(BcvUserControl),
                new PropertyMetadata(new BookChapterVerseViewModel()));
        public BookChapterVerseViewModel CurrentBcv
        {
            get => (BookChapterVerseViewModel)GetValue(CurrentBcvProperty);
            set
            {
                CalculateBooks();
                CalculateChapters();
                CalculateVerses();
                SetValue(CurrentBcvProperty, value);
            }
        }
        
        
        public static readonly DependencyProperty VerseChangeProperty =
            DependencyProperty.Register("VerseChange", typeof(string), typeof(BcvUserControl),
                new PropertyMetadata(""));
        public string VerseChange
        {
            get => (string)GetValue(VerseChangeProperty);
            set
            {
                SetValue(VerseChangeProperty, value);
            }
        }

        
        public static readonly DependencyProperty IsControlEnabledProperty =
            DependencyProperty.Register("IsControlEnabled", typeof(bool), typeof(BcvUserControl),
                new PropertyMetadata(true));
        public bool IsControlEnabled
        {
            get => (bool)GetValue(IsControlEnabledProperty);
            set => SetValue(IsControlEnabledProperty, value);
        }

        
        public static readonly DependencyProperty ShowOffsetControlProperty =
            DependencyProperty.Register("ShowOffsetControl", typeof(bool), typeof(BcvUserControl),
                new PropertyMetadata(true));
        public bool ShowOffsetControl
        {
            get => (bool)GetValue(ShowOffsetControlProperty);
            set => SetValue(ShowOffsetControlProperty, value);
        }



        public static readonly DependencyProperty ShowHeaderProperty =
            DependencyProperty.Register("ShowHeader", typeof(bool), typeof(BcvUserControl),
                new PropertyMetadata(true));
        public bool ShowHeader
        {
            get => (bool)GetValue(ShowHeaderProperty);
            set => SetValue(ShowHeaderProperty, value);
        }


        
        public static readonly DependencyProperty VerseRangeProperty =
            DependencyProperty.Register("VerseRange", typeof(int), typeof(BcvUserControl),
                new PropertyMetadata(1));
        public int VerseRange
        {
            get => (int)GetValue(VerseRangeProperty);
            set => SetValue(VerseRangeProperty, value);
        }


        public static readonly DependencyProperty BcvDictionaryProperty =
            DependencyProperty.Register("BcvDictionary", typeof(Dictionary<string, string>), typeof(BcvUserControl),
                new PropertyMetadata(new Dictionary<string, string>()));
        public Dictionary<string, string> BcvDictionary
        {
            get => (Dictionary<string, string>)GetValue(BcvDictionaryProperty);
            set => SetValue(BcvDictionaryProperty, value);
        }



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

        private void CalculateBooks()
        {
            CurrentBcv.BibleBookList?.Clear();

            var books = BcvDictionary.Values.GroupBy(b => b.Substring(0, 3))
                .Select(g => g.First())
                .ToList();

            foreach (var book in books)
            {
                var bookId = book.Substring(0, 3);

                var bookName = BookChapterVerseViewModel.GetShortBookNameFromBookNum(bookId);

                CurrentBcv.BibleBookList?.Add(bookName);
            }

        }

        private void CalculateChapters()
        {
            // CHAPTERS
            var bookId = CurrentBcv.Book;
            var chapters = BcvDictionary.Values.Where(b => bookId != null && b.StartsWith(bookId)).ToList();
            for (int i = 0; i < chapters.Count; i++)
            {
                chapters[i] = chapters[i].Substring(3, 3);
            }

            chapters = chapters.DistinctBy(v => v).ToList().OrderBy(b => b).ToList();
            // invoke to get it to run in STA mode
            System.Windows.Application.Current.Dispatcher.Invoke(delegate
            {
                List<int> chapterNumbers = new List<int>();
                foreach (var chapter in chapters)
                {
                    chapterNumbers.Add(Convert.ToInt16(chapter));
                }

                CurrentBcv.ChapterNumbers = chapterNumbers;
            });
        }

        private void CalculateVerses()
        {
            // VERSES
            var bookId = CurrentBcv.Book;
            var chapId = CurrentBcv.ChapterIdText;
            var verses = BcvDictionary.Values.Where(b => b.StartsWith(bookId + chapId)).ToList();

            for (int i = 0; i < verses.Count; i++)
            {
                verses[i] = verses[i].Substring(6);
            }

            verses = verses.DistinctBy(v => v).ToList().OrderBy(b => b).ToList();
            // invoke to get it to run in STA mode
            System.Windows.Application.Current.Dispatcher.Invoke(delegate
            {
                List<int> verseNumbers = new List<int>();
                foreach (var verse in verses)
                {
                    verseNumbers.Add(Convert.ToInt16(verse));
                }

                CurrentBcv.VerseNumbers = verseNumbers;
            });
        }


        // Declare the event
        public event PropertyChangedEventHandler PropertyChanged;

        // Create the OnPropertyChanged method to raise the event
        // The calling member's name will be used as the parameter.
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        #endregion
    }
}
