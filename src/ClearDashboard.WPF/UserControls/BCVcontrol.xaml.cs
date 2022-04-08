using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ClearDashboard.Common.Models;

namespace ClearDashboard.Wpf.UserControls
{
    /// <summary>
    /// Interaction logic for BCVcontrol.xaml
    /// </summary>
    public partial class BCVcontrol : UserControl
    {
        //private SantaFeMessageHandler santaFeMessageHandler;

        public static readonly DependencyProperty _isRtl =
            DependencyProperty.Register("IsRtl", typeof(bool), typeof(BCVcontrol),
            new PropertyMetadata(false));

        public bool IsRtl
        {
            get => (bool)GetValue(_isRtl);
            set => SetValue(_isRtl, value);
        }

        public static readonly DependencyProperty _currentBCV =
            DependencyProperty.Register("CurrentBCV", typeof(BookChapterVerse), typeof(BCVcontrol),
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
            DependencyProperty.Register("BookNames", typeof(ObservableCollection<string>), typeof(BCVcontrol),
            new PropertyMetadata(new ObservableCollection<string>()));

        public ObservableCollection<string> BookNames
        {
            get => (ObservableCollection<string>)GetValue(_bookNames);
            set => SetValue(_bookNames, value);
        }

        public static readonly DependencyProperty _chapNums =
            DependencyProperty.Register("ChapNums", typeof(ObservableCollection<int>), typeof(BCVcontrol),
            new PropertyMetadata(new ObservableCollection<int>()));

        public ObservableCollection<int> ChapNums
        {
            get => (ObservableCollection<int>)GetValue(_chapNums);
            set => SetValue(_chapNums, value);
        }

        public static readonly DependencyProperty _verseNums =
            DependencyProperty.Register("VerseNums", typeof(ObservableCollection<int>), typeof(BCVcontrol),
            new PropertyMetadata(new ObservableCollection<int>()));

        public ObservableCollection<int> VerseNums
        {
            get => (ObservableCollection<int>)GetValue(_verseNums);
            set => SetValue(_verseNums, value);
        }

        //public ScrollGroup ScrollGroupSelectedValue => (ScrollGroup)cboScrollGroup.SelectedItem;

        //private readonly List<ScrollGroup> ScrollGroups = ScrollGroupList.GetScrollGroupList();

        public BCVcontrol()
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

            //cboScrollGroup.ItemsSource = ScrollGroups;
            //cboScrollGroup.SelectedIndex = 0;

            // After the page is loaded, setup the event to watch for Paratext verse location changes.
            //santaFeMessageHandler = new SantaFeMessageHandler();
            //santaFeMessageHandler.VerseLocationChangedMsgEventHandler += new MsgEventHandler(VerseLocationMessageEventHandler);
        }

        /// <summary>
        ///  This is run when a new SantaFeMessage event happens.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        //private void VerseLocationMessageEventHandler(object source, VerseLocationChangedEventArgs e)
        //{
        //    IDictionary<string, string> verseInfo = e.EventInfo;
        //    string scriptureReference = verseInfo.First(f => f.Key.Equals("Scripture Reference", StringComparison.Ordinal)).Value;
        //    string scrollGroup = verseInfo.First(f => f.Key.Equals("Scroll Group", StringComparison.Ordinal)).Value;

        //    if (ScrollGroupSelectedValue != null && ScrollGroupSelectedValue.Value == scrollGroup)
        //    {
        //        _ = CurrentBCV.SetVerseFromId(scriptureReference);
        //    }
        //}

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

                //SantaFeScriptureLocationUpdate();
            }
        }

        //private void SantaFeScriptureLocationUpdate()
        //{
        //    if (santaFeMessageHandler != null
        //        && ScrollGroupSelectedValue != null
        //        && !ScrollGroupSelectedValue.ItemName.Equals("NONE", StringComparison.OrdinalIgnoreCase)
        //        && CurrentBCV.Chapter != null
        //        && CurrentBCV.Verse != null)
        //    {
        //        santaFeMessageHandler.SetSantaFeScriptureReference(CurrentBCV.BookNum, (int)CurrentBCV.Chapter, (int)CurrentBCV.Verse, ScrollGroupSelectedValue.Value);
        //    }
        //}

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

    }
}
