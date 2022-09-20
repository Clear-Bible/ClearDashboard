using ClearDashboard.DAL.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;

namespace ClearDashboard.Wpf.Application.UserControls
{
    /// <summary>
    /// Interaction logic for BookChapterVerse.xaml
    /// </summary>
    public partial class BcvUserControl : INotifyPropertyChanged
    {
        #region Member Variables

        private bool InComingChangesStarted { get; set; }

        #endregion



        #region Observable Properties


        #region ParatextSync

        public static readonly DependencyProperty ParatextSyncProperty =
            DependencyProperty.Register(nameof(ParatextSync), typeof(bool), typeof(BcvUserControl),
                new PropertyMetadata(true, new PropertyChangedCallback(OnParatextSyncPropertyChanged)));

        private static void OnParatextSyncPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            BcvUserControl? userControl = d as BcvUserControl;
            userControl?.OnParatextSyncPropertyChanged(e);
        }

        private void OnParatextSyncPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            ParatextSync = (bool)e.NewValue;
        }

        public bool ParatextSync
        {
            get => (bool)GetValue(ParatextSyncProperty);
            set
            {
                SetValue(ParatextSyncProperty, value);
            }
        }

        #endregion ParatextSync



        #region Rtl

        public static readonly DependencyProperty IsRtlProperty =
            DependencyProperty.Register(nameof(IsRtl), typeof(bool), typeof(BcvUserControl),
                new PropertyMetadata(false));

        public bool IsRtl
        {
            get => (bool)GetValue(IsRtlProperty);
            set => SetValue(IsRtlProperty, value);
        }

        #endregion Rtl
        
        


        #region CurrentBcv

        public static readonly DependencyProperty CurrentBcvProperty =
            DependencyProperty.Register(nameof(CurrentBcv), typeof(BookChapterVerseViewModel), typeof(BcvUserControl),
                new PropertyMetadata(new BookChapterVerseViewModel(), new PropertyChangedCallback(OnCurrentBcvPropteryChanged)));

        private static void OnCurrentBcvPropteryChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            BcvUserControl? userControl = d as BcvUserControl;
            userControl?.OnCurrentBcvPropteryChanged(e);
        }

        private void OnCurrentBcvPropteryChanged(DependencyPropertyChangedEventArgs e)
        {
            CurrentBcv = (BookChapterVerseViewModel)e.NewValue;
        }

        public BookChapterVerseViewModel CurrentBcv
        {
            get => (BookChapterVerseViewModel)GetValue(CurrentBcvProperty);
            set
            {
                SetValue(CurrentBcvProperty, value);

                CalculateBooks();
                CalculateChapters();
                CalculateVerses();

                VerseChange = CurrentBcv.GetVerseId();
            }
        }

        #endregion CurrentBcv


        
        #region VerseChange

        public static readonly DependencyProperty VerseChangeProperty =
            DependencyProperty.Register(nameof(VerseChange), typeof(string), typeof(BcvUserControl),
                new PropertyMetadata("", new PropertyChangedCallback(OnVerseChangePropertyChanged)));

        private static void OnVerseChangePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            BcvUserControl? userControl = d as BcvUserControl;
            userControl?.OnVerseChangePropertyChanged(e);
        }

        private void OnVerseChangePropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            VerseChange = (string)e.NewValue;
        }
        public string VerseChange
        {
            get => (string)GetValue(VerseChangeProperty);
            set
            {
                SetValue(VerseChangeProperty, value);
            }
        }

        #endregion VerseChange


        #region IsControlEnabled

        public static readonly DependencyProperty IsControlEnabledProperty =
            DependencyProperty.Register(nameof(IsControlEnabled), typeof(bool), typeof(BcvUserControl),
                new PropertyMetadata(true));
        public bool IsControlEnabled
        {
            get => (bool)GetValue(IsControlEnabledProperty);
            set => SetValue(IsControlEnabledProperty, value);
        }

        #endregion IsControlEnabled


        #region IsControlMinimal
        
        public static readonly DependencyProperty IsControlMinimalProperty =
            DependencyProperty.Register(nameof(IsControlMinimal), typeof(bool), typeof(BcvUserControl),
                new PropertyMetadata(false));
        public bool IsControlMinimal
        {
            get => (bool)GetValue(IsControlMinimalProperty);
            set => SetValue(IsControlMinimalProperty, value);
        }

        #endregion IsControlMinimal


        #region ShowOffsetControl

        public static readonly DependencyProperty ShowOffsetControlProperty =
            DependencyProperty.Register(nameof(ShowOffsetControl), typeof(bool), typeof(BcvUserControl),
                new PropertyMetadata(true));
        public bool ShowOffsetControl
        {
            get => (bool)GetValue(ShowOffsetControlProperty);
            set => SetValue(ShowOffsetControlProperty, value);
        }

        #endregion ShowOffsetControl



        #region ShowHeader

        public static readonly DependencyProperty ShowHeaderProperty =
            DependencyProperty.Register(nameof(ShowHeader), typeof(bool), typeof(BcvUserControl),
                new PropertyMetadata(true, new PropertyChangedCallback(OnShowHeaderPropteryChanged)));

        private static void OnShowHeaderPropteryChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            BcvUserControl? userControl = d as BcvUserControl;
            userControl?.OnShowHeaderPropteryChanged(e);
        }

        private void OnShowHeaderPropteryChanged(DependencyPropertyChangedEventArgs e)
        {
            ShowHeader = (bool)e.NewValue;
        }

        public bool ShowHeader
        {
            get => (bool)GetValue(ShowHeaderProperty);
            set => SetValue(ShowHeaderProperty, value);
        }

        #endregion ShowHeader



        #region VerseRange

        public static readonly DependencyProperty VerseRangeProperty =
            DependencyProperty.Register(nameof(VerseRange), typeof(int), typeof(BcvUserControl),
                new PropertyMetadata(1,
                new PropertyChangedCallback(OnVerseRangePropertyChanged)));

        private static void OnVerseRangePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            BcvUserControl? userControl = d as BcvUserControl;
            userControl?.OnVerseRangePropertyChanged(e);
        }

        private void OnVerseRangePropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            VerseRange = (int)e.NewValue;
        }

        public int VerseRange
        {
            get => (int)GetValue(VerseRangeProperty);
            set => SetValue(VerseRangeProperty, value);
        }

        # endregion VerseRange

        

        #region BcvDictionary

        public static readonly DependencyProperty BcvDictionaryProperty = DependencyProperty.Register(
            nameof(BcvDictionary), typeof(Dictionary<string, string>), typeof(BcvUserControl),
            new PropertyMetadata(new Dictionary<string, string>(),
                new PropertyChangedCallback(OnBcvDictionaryChanged)));

        private static void OnBcvDictionaryChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            BcvUserControl? userControl = d as BcvUserControl;
            userControl?.OnBcvDictionaryChanged(e);
        }

        private void OnBcvDictionaryChanged(DependencyPropertyChangedEventArgs e)
        {
            BcvDictionary = (Dictionary<string, string>)e.NewValue;
        }

        public Dictionary<string, string> BcvDictionary
        {
            get => (Dictionary<string, string>)GetValue(BcvDictionaryProperty);
            set
            {
                SetValue(BcvDictionaryProperty, value);
            }
        }

        #endregion BcvDictionary

        #endregion Observable Properties


        
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

                VerseChange = CurrentBcv.GetVerseId();
            }
        }

        private void BookDownArrow_Click(object sender, RoutedEventArgs e)
        {
            CboBook.SelectedIndex += 1;

            VerseChange = CurrentBcv.GetVerseId();
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

            VerseChange = CurrentBcv.GetVerseId();
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

            VerseChange = CurrentBcv.GetVerseId();
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

            VerseChange = CurrentBcv.GetVerseId();
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
            if (BcvDictionary is null)
            {
                return;
            }

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
            if (BcvDictionary is null)
            {
                return;
            }

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
            if (BcvDictionary is null)
            {
                return;
            }

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

        private void CboBook_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (CurrentBcv.GetVerseId() != VerseChange)
            {
                bool somethingChanged = false;

                // book switch so find the first chapter and verse for that book
                var verseId = BcvDictionary.Values.First(b => b[..3] == CurrentBcv.Book);
                if (verseId != "")
                {
                    InComingChangesStarted = true;
                    CurrentBcv.SetVerseFromId(verseId);

                    CalculateChapters();
                    CalculateVerses();
                    InComingChangesStarted = false;
                    somethingChanged = true;
                }

                if (somethingChanged)
                {
                    VerseChange = CurrentBcv.GetVerseId();
                }
            }
        }

        private void CboChapter_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (CurrentBcv.GetVerseId() != VerseChange)
            {
                bool somethingChanged = false;
                var BBBCCC = CurrentBcv.Book + CurrentBcv.ChapterIdText;

                // chapter switch so find the first verse for that book and chapter
                var verseId = BcvDictionary.Values.First(b => b.Substring(0, 6) == BBBCCC);
                if (verseId != "")
                {
                    InComingChangesStarted = true;
                    CurrentBcv.SetVerseFromId(verseId);

                    CalculateVerses();
                    InComingChangesStarted = false;
                    somethingChanged = true;
                }

                if (somethingChanged)
                {
                    VerseChange = CurrentBcv.GetVerseId();
                }
            }
        }


        private void CboVerse_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (CurrentBcv.GetVerseId() != VerseChange)
            {
                CurrentBcv.SetVerseFromId(CurrentBcv.BBBCCCVVV);
                VerseChange = CurrentBcv.GetVerseId();
            }
        }





        // Declare the event
        public event PropertyChangedEventHandler? PropertyChanged;

        // Create the OnPropertyChanged method to raise the event
        // The calling member's name will be used as the parameter.
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }



        #endregion


    }
}
