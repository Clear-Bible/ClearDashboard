using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using ClearDashboard.Wpf.Application.Dialogs;
using ClearDashboard.Wpf.Application.Events;

namespace ClearDashboard.Wpf.Application.Views.EnhancedView
{
    /// <summary>
    /// Interaction logic for EnhancedView.xaml
    /// </summary>
    public partial class EnhancedView : UserControl
    {

        // TODO:  this needs to be moved into VerseAwareEnhancedViewItemViewModel.
        public void TranslationClicked(object sender, RoutedEventArgs routedEventArgs)
        {
            Task.Run(() => TranslationClickedAsync(routedEventArgs as TranslationEventArgs ?? throw new InvalidOperationException()).GetAwaiter());
        }

        public async Task TranslationClickedAsync(TranslationEventArgs args)
        {
            void ShowTranslationSelectionDialog()
            {
                var dialog = new TranslationSelectionDialog(args.TokenDisplay!, args.InterlinearDisplay!)
                {
                    Owner = Window.GetWindow(this),
                };
                dialog.ShowDialog();
            }
            await System.Windows.Application.Current.Dispatcher.InvokeAsync(ShowTranslationSelectionDialog);
        }

        public EnhancedView()
        {
            InitializeComponent();
        }

        private void VerseContentControl_OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scrollViewer = Helpers.Helpers.GetChildOfType<ScrollViewer>(OuterListView);
            scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - e.Delta/3);
            e.Handled = true;
        }

        private void ProjectDesignSurfaceExpander_OnExpanded(object sender, RoutedEventArgs e)
        {
            if (NotesExpander != null && NotesControl != null)
            {
                NotesExpander.Width = 300;
                NotesControl.Width = 300;
            }

            NotesColumn.Width = new GridLength(300, GridUnitType.Auto);
            NotesSplitter.Visibility = Visibility.Visible;
        }

        private void ProjectDesignSurfaceExpander_OnCollapsed(object sender, RoutedEventArgs e)
        {
            NotesExpander.Width = 24;
            NotesControl.Width = 24;

            NotesColumn.Width = new GridLength(1, GridUnitType.Auto);
            
            NotesSplitter.Visibility = Visibility.Collapsed;
        }

        private void ProjectDesignSurfaceSplitter_OnDragDelta(object sender, DragDeltaEventArgs e)
        {
            NotesExpander.Width = NotesColumn.ActualWidth;
            if (NotesColumn.ActualWidth - 50 >= 0)
            {
                NotesControl.Width = NotesColumn.ActualWidth - 50;
            }
            else
            {
                NotesControl.Width = 0;
            }

        }
    }
}
