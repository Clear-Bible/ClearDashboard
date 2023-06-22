using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace ClearDashboard.Wpf.Application.UserControls
{

    public class Book
    {
        public int Number { get; set; }
        public string? Code { get; set; }
    }

    public class BookComparer : IEqualityComparer<Book>
    {
        public bool Equals(Book x, Book y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            if (x.GetType() != y.GetType()) return false;
            return x.Number == y.Number && x.Code == y.Code;
        }

        public int GetHashCode(Book obj)
        {
            return HashCode.Combine(obj.Number, obj.Code);
        }
    }
    /// <summary>
    /// Interaction logic for BookSelector.xaml
    /// </summary>
    // ReSharper disable once RedundantExtendsListEntry
    public partial class BookSelector : UserControl
    {
        public event EventHandler<SelectionChangedEventArgs>? SelectionChanged;

        public BookSelector()
        {
            InitializeComponent();
        }

        private void BookCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectionChanged?.Invoke(this, e);
        }

        private void LeftButton_Click(object sender, RoutedEventArgs e)
        {
            if (BookCombobox.SelectedIndex > 0)
            {
                BookCombobox.SelectedIndex -= 1;
            }
        }

        private void RightButton_Click(object sender, RoutedEventArgs e)
        {
            if (BookCombobox.SelectedIndex < BookCombobox.Items.Count -1)
            {
                BookCombobox.SelectedIndex += 1;
            }
        }

        public static readonly DependencyProperty CurrentBookProperty = DependencyProperty.Register(
            nameof(CurrentBook), typeof(Book), typeof(BookSelector),
            new PropertyMetadata(new Book(), OnCurrentBookChanged));

        private static void OnCurrentBookChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var userControl = d as BookSelector;
            userControl?.OnCurrentBookChanged(e);
        }

        private void OnCurrentBookChanged(DependencyPropertyChangedEventArgs e)
        {
            CurrentBook = (Book)e.NewValue;
        }

        public Book CurrentBook
        {
            get => (Book)GetValue(CurrentBookProperty);
            set => SetValue(CurrentBookProperty, value);
        }

        public static readonly DependencyProperty BooksProperty = DependencyProperty.Register(
            nameof(Books), typeof(List<Book>), typeof(BookSelector),
            new PropertyMetadata(new List<Book>(), OnBooksChanged));

        private static void OnBooksChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var userControl = d as BookSelector;
            userControl?.OnBooksChanged(e);
        }

        // Declare the event
        public event PropertyChangedEventHandler? PropertyChanged;

        // Create the OnPropertyChanged method to raise the event
        // The calling member's name will be used as the parameter.
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private void OnBooksChanged(DependencyPropertyChangedEventArgs e)
        {
            Books = (List<Book>)e.NewValue;
        }

        public List<Book> Books
        {
            get => (List<Book>)GetValue(BooksProperty);
            set => SetValue(BooksProperty, value);
        }
    }
}
