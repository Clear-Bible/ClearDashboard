using System;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.Wpf.Application.ViewModels.ParatextViews;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using SIL.IO.FileLock;
using ClearDashboard.DAL.ViewModels;
using ClearDashboard.DAL.WpfViewModels;
using ClearDashboard.Wpf.Application.Helpers;

namespace ClearDashboard.Wpf.Application.Views.ParatextViews
{
    /// <summary>
    /// Interaction logic for BiblicalTermsView.xaml
    /// </summary>
    public partial class BiblicalTermsView : UserControl
    {
        private BiblicalTermsViewModel _vm;
        public BiblicalTermsView()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            _vm = (BiblicalTermsViewModel)DataContext;

            // listen for changes to the lower listview to make it scroll back 
            // to the top
            INotifyPropertyChanged viewModel = (INotifyPropertyChanged)this.DataContext;
            viewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName.Equals("SelectedItemVerses"))
                {
                    if (SelectedItemVerses.Items.Count > 0)
                    {
                        SelectedItemVerses.ScrollIntoView(SelectedItemVerses.Items[0]);
                    }

                    return;
                    // execute code here.
                }
                if (args.PropertyName.Equals("WindowFlowDirection"))
                {
                    this.FlowDirection = _vm.WindowFlowDirection;
                }

            };
        }

        private void CopyText_OnClick(object sender, RoutedEventArgs e)
        {
            string copyText=String.Empty;

            if (sender is MenuItem menuItem)
            {
                var parent = menuItem.Parent;
                var contextMenu = (ContextMenu)parent;
                sender = contextMenu.PlacementTarget;
            }
            
            if (sender is DataGrid grid)
            {
                var columnIndex = grid.CurrentColumn!=null? grid.CurrentColumn.DisplayIndex : 6;

                var cells = grid.SelectedCells;
                if (cells.Count != 0)
                {
                    var selectedItem = cells[0].Item;
                    var BiblicalTermsItem = (BiblicalTermsData)selectedItem;

                    switch (columnIndex)
                    {
                        case (0):
                            copyText = BiblicalTermsItem.Id;
                            break;
                        case (1):
                            copyText = BiblicalTermsItem.SemanticDomain;
                            break;
                        case (2):
                            copyText = BiblicalTermsItem.Gloss;
                            break;
                        case (3):
                            copyText = BiblicalTermsItem.Counts;
                            break;
                        case (4):
                            copyText = BiblicalTermsItem.Found.ToString();
                            break;
                        case (5):
                            sender = sender as ListBox;
                            break;
                        default:
                            copyText = BiblicalTermsItem.Gloss;
                            break;
                    }
                }
            }
            else if (sender is ListBox listBox && listBox == SelectedItemVerseRenderings)
            {
                copyText = listBox.SelectedItem.ToString();
            }
            else if (sender is ListBox verseListBox && verseListBox.SelectedItem is VerseViewModel)
            {
                var verseViewModel = verseListBox.SelectedItem as VerseViewModel;
                copyText = verseViewModel.VerseText;
            }
            else if (sender is ListView listView && listView == SelectedItemVerses && listView.SelectedItem is VerseViewModel verse)
            {
                copyText = verse.VerseText;
            }
            else if (sender is ScrollViewer scrollViewer)
            {
                var verseViewModel = scrollViewer.DataContext as VerseViewModel;
                copyText = verseViewModel.VerseText;
            }

            if (sender is ListBox listTwo && listTwo.SelectedItem is RenderingStringParts)
            {
                var renderingStringParts = listTwo.SelectedItem as RenderingStringParts;
                copyText = renderingStringParts.RenderingString;
            }

            Clipboard.SetText(copyText);
        }

        private void AllowScrolling_OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            
            if (sender == SelectedItemVerses)
            {
                ScrollViewer scrollViewer = Helpers.Helpers.GetChildOfType<ScrollViewer>(SelectedItemVerses);
                scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - e.Delta/3);
            }

            else if (sender == gridVerses || sender is ListBox)
            {
                ScrollViewer scrollViewer = Helpers.Helpers.GetChildOfType<ScrollViewer>(gridVerses);
                scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - e.Delta/40);
            }

            e.Handled = true;
        }

        private void FindText_OnClick(object sender, ExecutedRoutedEventArgs e)
        {
            FilterText.Focus();
        }
    }
}
