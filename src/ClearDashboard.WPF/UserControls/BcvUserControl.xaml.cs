using System;
using System.Collections.Generic;
using ClearDashboard.Common.Models;
using ClearDashboard.Wpf.ViewModels;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
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
            DependencyProperty.Register("CurrentBcv", typeof(BookChapterVerse), typeof(BcvUserControl),
                new PropertyMetadata(new BookChapterVerse()));

        public BookChapterVerse CurrentBcv
        {
            get => (BookChapterVerse)GetValue(_currentBcv);
            set
            {
                SetValue(_currentBcv, value);
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

        #endregion

        #region Constructor
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

        #endregion

        #region Methods

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

        #endregion
    }
}
