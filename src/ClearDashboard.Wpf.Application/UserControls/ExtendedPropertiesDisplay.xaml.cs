using Caliburn.Micro;
using ClearDashboard.Wpf.Application.Events;
using ClearDashboard.Wpf.Application.ViewModels.Display;
using SIL.Xml;
using System;
using System.Collections.Generic;
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
using System.Xml.Linq;

namespace ClearDashboard.Wpf.Application.UserControls
{
    /// <summary>
    /// Interaction logic for ExtendedPropertiesDisplay.xaml
    /// </summary>
    public partial class ExtendedPropertiesDisplay : UserControl
    {
        public ExtendedPropertiesDisplay()
        {
            InitializeComponent();

            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        //public TokenDisplayViewModel TokenDisplayViewModel => (TokenDisplayViewModel)DataContext;

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            //TokenDisplayViewModel.PropertyChanged += TokenDisplayViewModelPropertyChanged;
            CalculateLayout();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            //TokenDisplayViewModel.PropertyChanged -= TokenDisplayViewModelPropertyChanged;
        }

        /*
        private void TokenDisplayViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            CalculateLayout();
        }
        */

        public static readonly DependencyProperty ExtendedPropertiesXmlProperty = DependencyProperty.Register("ExtendedPropertiesXml", typeof(string), typeof(ExtendedPropertiesDisplay));
        public string? ExtendedPropertiesXml
        {
            get => (string)GetValue(ExtendedPropertiesXmlProperty);
            set => SetValue(ExtendedPropertiesXmlProperty, value);
        }

        public static readonly DependencyProperty ExtendedPropertiesXElementProperty = DependencyProperty.Register("ExtendedPropertiesXElement", typeof(XElement), typeof(ExtendedPropertiesDisplay));
        public XElement? ExtendedPropertiesXElement
        {
            get => (XElement)GetValue(ExtendedPropertiesXElementProperty);
            set => SetValue(ExtendedPropertiesXElementProperty, value);
        }
        /*
        private void CalculateLayout()
        {
            ExtendedPropertiesXElement = XElement.Parse(ExtendedPropertiesXml);

            BuildTreeViewItems(ExtendedPropertiesXElement)
                .Select(i => ExtendedPropertiesTree.Items.Add(i))
                .ToList();
        }

        private IEnumerable<TreeViewItem> BuildTreeViewItems(XElement xElement)
        {
            if (xElement.HasElements)
            {
                return xElement.Elements()
                    .Select(c =>
                    {
                        TreeViewItem childTreeNode = new TreeViewItem
                        {
                            Header = xElement.Name,
                            IsExpanded = true
                        };
                        BuildTreeViewItems(c)
                            .Select(i => childTreeNode.Items.Add(i))
                            .ToList();
                        return childTreeNode;
                    })
                    .ToList();
            }
            else
            {
                var treeViewItem = new TreeViewItem
                {
                    Header = $"{xElement.Name}:  {xElement.Value}",
                    IsExpanded = true
                };
                return new List<TreeViewItem>() { treeViewItem };
            }
        }
        */

        private void CalculateLayout()
        {
            ExtendedPropertiesXElement = XElement.Parse(ExtendedPropertiesXml);

            ExtendedPropertiesXElement.Elements()
                .Select(e => ExtendedPropertiesTree.Items.Add(BuildTreeViewItems(e)))
                .ToList();
        }

        private TreeViewItem BuildTreeViewItems(XElement xElement)
        {
            TreeViewItem childTreeNode = new TreeViewItem
            {
                IsExpanded = true
            };

            if (xElement.HasElements)
            {
                childTreeNode.Header = $"{xElement.Name.LocalName}";
                xElement.Elements()
                    .Select(e => childTreeNode.Items.Add(BuildTreeViewItems(e)))
                    .ToList();
            }
            else
            {
                childTreeNode.Header = $"{xElement.Name.LocalName}:  {xElement.Value}";
            }
            return childTreeNode;
        }
    }
}
