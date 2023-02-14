using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
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
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.Wpf.Application.ViewModels.ParatextViews;

namespace ClearDashboard.Wpf.Application.Views.ParatextViews
{
    /// <summary>
    /// Interaction logic for PinsView.xaml
    /// </summary>
    public partial class PinsView : UserControl
    {
        public PinsView()
        {
            InitializeComponent();
        }

        private void CopyText_OnClick(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem)
            {
                var parent = menuItem.Parent;
                var contextMenu = (ContextMenu)parent;
                sender = contextMenu.PlacementTarget;
            }
            
            var item = (DataGrid)sender;

            var columnIndex = item.SelectedCells[0].Column.DisplayIndex;

            var cells = item.SelectedCells;
            var selectedItem = cells[0].Item;
            var pinsTableItem = (PinsDataTable)selectedItem;
            string copyText;
            switch (columnIndex)
            {
                case (0):
                    copyText = pinsTableItem.XmlSource;
                    break;
                case (1):
                    copyText = pinsTableItem.SimpRefs;
                    break;
                case (2):
                    copyText = pinsTableItem.Source;
                    break;
                case (3):
                    copyText = pinsTableItem.Gloss;
                    break;
                case (4):
                    copyText = pinsTableItem.OriginID;
                    break;
                case (5):
                    copyText = pinsTableItem.Lang;
                    break;
                case (6):
                    copyText = pinsTableItem.Phrase;
                    break;
                case (7):
                    copyText = pinsTableItem.Word;
                    break;
                case (8):
                    copyText = pinsTableItem.Prefix;
                    break;
                case (9):
                    copyText = pinsTableItem.Stem;
                    break;
                case (10):
                    copyText = pinsTableItem.Suffix;
                    break;
                case (11):
                    copyText = pinsTableItem.Notes;
                    break;
                default:
                    copyText = pinsTableItem.Source;
                    break;
            }
            Clipboard.SetText(copyText);
        }
    }
}
