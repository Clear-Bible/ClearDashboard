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

            ClearTree(xmlTreeView);
            BuildTree(xmlTreeView);
        }

        public static readonly DependencyProperty IsExpandedProperty =
            DependencyProperty.Register(
              name: "IsExpanded",
              propertyType: typeof(bool),
              ownerType: typeof(ExtendedPropertiesTreeView),
              typeMetadata: new FrameworkPropertyMetadata(true,
                  new PropertyChangedCallback(IsExpandedPropertyChanged))
            );

        public bool IsExpanded
        {
            get => (bool)GetValue(IsExpandedProperty);
            set => SetValue(IsExpandedProperty, value);
        }

        static void IsExpandedPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ExtendedPropertiesTreeView xmlTreeView = (ExtendedPropertiesTreeView)sender;

            ClearTree(xmlTreeView);
            BuildTree(xmlTreeView);
        }

        static void ClearTree(ExtendedPropertiesTreeView xmlTreeView)
        {
            xmlTreeView.Items.Clear();
        }
        static void BuildTree(ExtendedPropertiesTreeView xmlTreeView)
        {
            try
            {
                xmlTreeView.Visibility = Visibility.Visible;
                XElement.Parse(xmlTreeView.ExtendedPropertiesXml!)
                    .Elements()
                    .Select(e => xmlTreeView.Items.Add(BuildTreeViewItems(e, xmlTreeView.IsExpanded)))
                    .ToList();
            }
            catch (Exception)
            {
                throw new InvalidDataEngineException(name: "Xml", value: xmlTreeView.ExtendedPropertiesXml ?? "null");
            }
        }
        private static TreeViewItem BuildTreeViewItems(XElement xElement, bool isExpanded)
        {
            TreeViewItem childTreeNode = new TreeViewItem
            {
                IsExpanded = isExpanded
            };

            if (xElement.HasElements)
            {
                childTreeNode.Header = $"{xElement.Name.LocalName}";
                xElement.Elements()
                    .Select(e => childTreeNode.Items.Add(BuildTreeViewItems(e, isExpanded)))
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
