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
              name: "ExtendePropertiesXml",
              propertyType: typeof(string),
              ownerType: typeof(ExtendedPropertiesTreeView),
              typeMetadata: new FrameworkPropertyMetadata("",
                  new PropertyChangedCallback(ExtendedPropertiesXmlPropertyChanged))
            );

        public string? ExtendePropertiesXml
        {
            get { return (string)GetValue(ExtendedPropertiesXmlProperty); }
            set { SetValue(ExtendedPropertiesXmlProperty, value); }
        }
        static void ExtendedPropertiesXmlPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ExtendedPropertiesTreeView xmlTreeView = (ExtendedPropertiesTreeView)sender;

            if (xmlTreeView.ExtendePropertiesXml == null)
            {
                xmlTreeView.Visibility = Visibility.Collapsed;
                return;
            }

            try
            {
                xmlTreeView.Visibility = Visibility.Visible;
                XElement.Parse(xmlTreeView.ExtendePropertiesXml!)
                    .Elements()
                    .Select(e => xmlTreeView.Items.Add(BuildTreeViewItems(e)))
                    .ToList();
            }
            catch (Exception)
            {
                throw new InvalidDataEngineException(name: "Xml", value: xmlTreeView.ExtendePropertiesXml ?? "null");
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
