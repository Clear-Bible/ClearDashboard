using ClearBible.Engine.Utils;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.Wpf.Application.Collections;
using ClearDashboard.Wpf.Application.Events;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ClearDashboard.Wpf.Application.UserControls
{
    /// <summary>
    /// Interaction logic for NoteRepliesDisplay.xaml
    /// </summary>
    public partial class NoteRepliesDisplay : UserControl
    {
        public NoteRepliesDisplay()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty NoteViewModelWithRepliesProperty = DependencyProperty.Register(nameof(NoteViewModelWithReplies), typeof(NoteViewModel), typeof(NoteRepliesDisplay));
        public NoteViewModel NoteViewModelWithReplies
        {
            get
            {
                return (NoteViewModel)GetValue(NoteViewModelWithRepliesProperty);
            }
            set
            {
                SetValue(NoteViewModelWithRepliesProperty, value);
                NoteViewModelWithRepliesCollectionView = CollectionViewSource.GetDefaultView(value.Replies);
                NoteViewModelWithRepliesCollectionView.SortDescriptions.Clear();
                NoteViewModelWithRepliesCollectionView.SortDescriptions.Add(new SortDescription("Created", ListSortDirection.Ascending));
            }
        }

        public static readonly DependencyProperty NoteViewModelWithRepliesCollectionViewProperty = DependencyProperty.Register(nameof(NoteViewModelWithRepliesCollectionView), typeof(ICollectionView), typeof(NoteRepliesDisplay));
        public ICollectionView NoteViewModelWithRepliesCollectionView
        {
            get => (ICollectionView)GetValue(NoteViewModelWithRepliesCollectionViewProperty);
            set => SetValue(NoteViewModelWithRepliesCollectionViewProperty, value);
        }

        public event RoutedEventHandler NoteReplyAdd
        {
            add => AddHandler(NoteReplyAddEvent, value);
            remove => RemoveHandler(NoteReplyAddEvent, value);
        }

        public static readonly RoutedEvent NoteReplyAddEvent = EventManager.RegisterRoutedEvent
            ("NoteReplyAdd", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NoteRepliesDisplay));

        public event RoutedEventHandler NoteSeen
        {
            add => AddHandler(NoteSeenEvent, value);
            remove => RemoveHandler(NoteSeenEvent, value);
        }
        public static readonly RoutedEvent NoteSeenEvent = EventManager.RegisterRoutedEvent
            ("NoteSeen", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NoteRepliesDisplay));

        private async void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            var checkBox = e.Source as CheckBox;
            var noteViewModel = checkBox?.DataContext as NoteViewModel;
            if (noteViewModel != null)
            {
                RaiseEvent(new NoteSeenEventArgs
                {
                    RoutedEvent = NoteSeenEvent,
                    Seen = true,
                    NoteViewModel = noteViewModel
                });
            }
        }

        private void CheckBox_UnChecked(object sender, RoutedEventArgs e)
        {
            var checkBox = e.Source as CheckBox;
            var noteViewModel = checkBox?.DataContext as NoteViewModel;
            if (noteViewModel != null)
            {
                RaiseEvent(new NoteSeenEventArgs
                {
                    RoutedEvent = NoteSeenEvent,
                    Seen = false,
                    NoteViewModel = noteViewModel
                });
            }
        }
        private void AddReplyText_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                RaiseEvent(new NoteReplyAddEventArgs
                {
                    RoutedEvent = NoteReplyAddEvent,
                    Text = ((TextBox)e.Source).Text,
                    NoteViewModelWithReplies = NoteViewModelWithReplies
                });
            }
        }
    }
}
