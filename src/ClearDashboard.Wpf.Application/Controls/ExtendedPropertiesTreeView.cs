using ClearBible.Engine.Exceptions;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;

namespace ClearDashboard.Wpf.Application.Controls
{
    public class ExtendedPropertiesTreeView : TreeView
    {
        public ExtendedPropertiesTreeView()
        {
            Visibility = Visibility.Collapsed;
        }

        public static readonly DependencyProperty ExtendedPropertiesXmlProperty =
            DependencyProperty.Register(
              name: "ExtendedPropertiesXml",
              propertyType: typeof(string),
              ownerType: typeof(ExtendedPropertiesTreeView),
              typeMetadata: new FrameworkPropertyMetadata("",
                  new PropertyChangedCallback(ExtendedPropertiesXmlPropertyChanged))
            );

        public string? ExtendedPropertiesXml
        {
            get => (string)GetValue(ExtendedPropertiesXmlProperty);
            set => SetValue(ExtendedPropertiesXmlProperty, value);
        }
        static void ExtendedPropertiesXmlPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ExtendedPropertiesTreeView xmlTreeView = (ExtendedPropertiesTreeView)sender;

            if (xmlTreeView.ExtendedPropertiesXml == null)
            {
                xmlTreeView.Visibility = Visibility.Collapsed;
                return;
            }

            try
            {
                xmlTreeView.Visibility = Visibility.Visible;
                XElement.Parse(xmlTreeView.ExtendedPropertiesXml!)
                    .Elements()
                    .Select(e => xmlTreeView.Items.Add(BuildTreeViewItems(e)))
                    .ToList();
            }
            catch (Exception)
            {
                throw new InvalidDataEngineException(name: "Xml", value: xmlTreeView.ExtendedPropertiesXml ?? "null");
            }
        }

        private static TreeViewItem BuildTreeViewItems(XElement xElement)
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
