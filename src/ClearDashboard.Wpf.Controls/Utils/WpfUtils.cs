using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;

namespace ClearDashboard.Wpf.Controls.Utils
{
    /// <summary>
    /// This class contains helper functions for dealing with WPF.
    /// </summary>
    public static class WpfUtils
    {
        /// <summary>
        /// Search up the element tree to find the Parent window for 'element'.
        /// Returns null if the 'element' is not attached to a window.
        /// </summary>
        public static Window FindParentWindow(FrameworkElement element)
        {
            if (element.Parent == null)
            {
                return null;
            }

            if (element.Parent is Window window)
            {
                return window;
            }

            if (element.Parent is FrameworkElement parentElement)
            {
                return FindParentWindow(parentElement);
            }

            return null;
        }

        public static FrameworkElement FindParentWithDataContextAndName<TDataContext>(FrameworkElement childElement, string name)
            where TDataContext : class
        {
            var parent = (FrameworkElement)childElement.Parent;
            if (parent != null)
            {
                if (parent.DataContext is TDataContext data)
                {
                    if (parent.Name == name)
                    {
                        return parent;
                    }
                }

                parent = FindParentWithDataContextAndName<TDataContext>(parent, name);
                if (parent != null)
                {
                    return parent;
                }
            }

            parent = (FrameworkElement)childElement.TemplatedParent;
            if (parent != null)
            {
                if (parent.DataContext is TDataContext data)
                {
                    if (parent.Name == name)
                    {
                        return parent;
                    }
                }

                parent = FindParentWithDataContextAndName<TDataContext>(parent, name);
                if (parent != null)
                {
                    return parent;
                }
            }

            return null;
        }

        public static FrameworkElement FindParentWithDataContext<TDataContext>(FrameworkElement childElement)
            where TDataContext : class
        {
            if (childElement.Parent != null)
            {
                if (((FrameworkElement)childElement.Parent).DataContext is TDataContext data)
                {
                    return (FrameworkElement)childElement.Parent;
                }

                var parent = FindParentWithDataContext<TDataContext>((FrameworkElement)childElement.Parent);
                if (parent != null)
                {
                    return parent;
                }
            }

            if (childElement.TemplatedParent != null)
            {
                if (((FrameworkElement)childElement.TemplatedParent).DataContext is TDataContext data)
                {
                    return (FrameworkElement)childElement.TemplatedParent;
                }

                var parent = FindParentWithDataContext<TDataContext>((FrameworkElement)childElement.TemplatedParent);
                if (parent != null)
                {
                    return parent;
                }
            }

            return null;
        }

        public static TParent FindVisualParentWithType<TParent>(FrameworkElement childElement)
            where TParent : class
        {
            var parentElement = (FrameworkElement)VisualTreeHelper.GetParent(childElement);
            if (parentElement != null)
            {
                if (parentElement is TParent parent)
                {
                    return parent;
                }

                return FindVisualParentWithType<TParent>(parentElement);
            }

            return null;
        }

        public static TParent FindParentWithType<TParent>(FrameworkElement childElement)
            where TParent : class
        {
            if (childElement.Parent != null)
            {
                if (childElement.Parent is TParent parent)
                {
                    return parent;
                }

                parent = FindParentWithType<TParent>((FrameworkElement)childElement.Parent);
                if (parent != null)
                {
                    return parent;
                }
            }

            if (childElement.TemplatedParent != null)
            {
                if (childElement.TemplatedParent is TParent parent)
                {
                    return parent;
                }

                parent = FindParentWithType<TParent>((FrameworkElement)childElement.TemplatedParent);
                if (parent != null)
                {
                    return parent;
                }
            }

            var parentElement = (FrameworkElement)VisualTreeHelper.GetParent(childElement);
            if (parentElement != null)
            {
                if (parentElement is TParent parent)
                {
                    return parent;
                }

                return FindParentWithType<TParent>(parentElement);
            }

            return null;
        }

        public static TParent FindParentWithTypeAndDataContext<TParent>(FrameworkElement childElement, object dataContext)
            where TParent : FrameworkElement
        {
            if (childElement.Parent != null)
            {
                if (childElement.Parent is TParent parent)
                {
                    if (parent.DataContext == dataContext)
                    {
                        return parent;
                    }
                }

                parent = FindParentWithTypeAndDataContext<TParent>((FrameworkElement)childElement.Parent, dataContext);
                if (parent != null)
                {
                    return parent;
                }
            }

            if (childElement.TemplatedParent != null)
            {
                if (childElement.TemplatedParent is TParent parent)
                {
                    if (parent.DataContext == dataContext)
                    {
                        return parent;
                    }
                }

                parent = FindParentWithTypeAndDataContext<TParent>((FrameworkElement)childElement.TemplatedParent, dataContext);
                if (parent != null)
                {
                    return parent;
                }
            }

            var parentElement = (FrameworkElement)VisualTreeHelper.GetParent(childElement);
            if (parentElement != null)
            {
                if (parentElement is TParent parent)
                {
                    return parent;
                }

                return FindParentWithType<TParent>(parentElement);
            }

            return null;
        }

        /// <summary>
        /// Hit test against the specified element for a child that has a data context
        /// of the specified type.
        /// Returns 'null' if nothing was 'hit'.
        /// Return the highest level element that matches the hit test.
        /// </summary>
        public static T HitTestHighestForDataContext<T>(FrameworkElement rootElement, Point point)
            where T : class
        {
            FrameworkElement hitFrameworkElement = null;
            return HitTestHighestForDataContext<T>(rootElement, point, out hitFrameworkElement);
        }

        /// <summary>
        /// Hit test against the specified element for a child that has a data context
        /// of the specified type.
        /// Returns 'null' if nothing was 'hit'.
        /// Return the highest level element that matches the hit test.
        /// </summary>
        public static T HitTestHighestForDataContext<T>(FrameworkElement rootElement,
                                                  Point point, out FrameworkElement hitFrameworkElement)
            where T : class
        {
            hitFrameworkElement = null;

            var hitData = HitTestForDataContext<T, FrameworkElement>(rootElement, point, out var hitElement);
            if (hitData == null)
            {
                return null;
            }

            hitFrameworkElement = hitElement;

            //
            // Find the highest level parent below root element that still matches the data context.
            while (hitElement != null && hitElement != rootElement &&
                   hitElement.DataContext == hitData)
            {
                hitFrameworkElement = hitElement;

                if (hitElement.Parent != null)
                {
                    hitElement = hitElement.Parent as FrameworkElement;
                    continue;
                }

                if (hitElement.TemplatedParent != null)
                {
                    hitElement = hitElement.TemplatedParent as FrameworkElement;
                    continue;
                }

                break;
            }

            return hitData;
        }


        /// <summary>
        /// Hit test for a specific data context and name.
        /// </summary>
        public static TDataContext HitTestForDataContextAndName<TDataContext, TElement>(FrameworkElement rootElement,
                                          Point point, string name, out TElement hitFrameworkElement)
            where TDataContext : class
            where TElement : FrameworkElement
        {
            TDataContext data = null;
            hitFrameworkElement = null;
            TElement frameworkElement = null;

            VisualTreeHelper.HitTest(
                    rootElement,
                    // Hit test filter.
                    null,
                    // Hit test result.
                    delegate (HitTestResult result)
                    {
                        frameworkElement = result.VisualHit as TElement;
                        if (frameworkElement != null)
                        {
                            data = frameworkElement.DataContext as TDataContext;
                            if (data != null)
                            {
                                if (frameworkElement.Name == name)
                                {
                                    return HitTestResultBehavior.Stop;
                                }
                            }
                        }

                        return HitTestResultBehavior.Continue;
                    },

                    new PointHitTestParameters(point));

            hitFrameworkElement = frameworkElement;
            return data;
        }


        /// <summary>
        /// Hit test against the specified element for a child that has a data context
        /// of the specified type.
        /// Returns 'null' if nothing was 'hit'.
        /// </summary>
        public static TDataContext HitTestForDataContext<TDataContext, TElement>(FrameworkElement rootElement,
                                          Point point, out TElement hitFrameworkElement)
            where TDataContext : class
            where TElement : FrameworkElement
        {
            TDataContext data = null;
            hitFrameworkElement = null;
            TElement frameworkElement = null;

            VisualTreeHelper.HitTest(
                    rootElement,
                    // Hit test filter.
                    null,
                    // Hit test result.
                    delegate (HitTestResult result)
                    {
                        frameworkElement = result.VisualHit as TElement;
                        if (frameworkElement != null)
                        {
                            data = frameworkElement.DataContext as TDataContext;
                            return data != null ? HitTestResultBehavior.Stop : HitTestResultBehavior.Continue;
                        }

                        return HitTestResultBehavior.Continue;
                    },

                    new PointHitTestParameters(point));

            hitFrameworkElement = frameworkElement;
            return data;
        }

        /// <summary>
        /// Find the ancestor of a particular element based on the type of the ancestor.
        /// </summary>
        public static T FindAncestor<T>(FrameworkElement element) where T : class
        {
            if (element.Parent != null)
            {
                if (element.Parent is T ancestor)
                {
                    return ancestor;
                }

                if (element.Parent is FrameworkElement parent)
                {
                    return FindAncestor<T>(parent);
                }
            }

            if (element.TemplatedParent != null)
            {
                if (element.TemplatedParent is T ancestor)
                {
                    return ancestor;
                }

                var parent = element.TemplatedParent as FrameworkElement;
                if (parent != null)
                {
                    return FindAncestor<T>(parent);
                }
            }

            var visualParent = VisualTreeHelper.GetParent(element);
            if (visualParent != null)
            {
                if (visualParent is T visualAncestor)
                {
                    return visualAncestor;
                }

                if (visualParent is FrameworkElement visualElement)
                {
                    return FindAncestor<T>(visualElement);
                }
            }

            return null;
        }

        /// <summary>
        /// Transform a point to an ancestors coordinate system.
        /// </summary>
        public static Point TransformPointToAncestor<T>(FrameworkElement element, Point point) where T : Visual
        {
            var ancestor = FindAncestor<T>(element);
            if (ancestor == null)
            {
                throw new ApplicationException("Find to find '" + typeof(T).Name + "' for element '" + element.GetType().Name + "'.");
            }

            return TransformPointToAncestor(ancestor, element, point);
        }

        /// <summary>
        /// Transform a point to an ancestors coordinate system.
        /// </summary>
        public static Point TransformPointToAncestor(Visual ancestor, FrameworkElement element, Point point)
        {
            return element.TransformToAncestor(ancestor).Transform(point);
        }

        /// <summary>
        /// Find the framework element with the specified name.
        /// </summary>
        public static TElement FindElementWithName<TElement>(Visual rootElement, string name)
            where TElement : FrameworkElement
        {
            if (rootElement is FrameworkElement rootFrameworkElement)
            {
                rootFrameworkElement.UpdateLayout();
            }

            var numChildren = VisualTreeHelper.GetChildrenCount(rootElement);
            for (var i = 0; i < numChildren; ++i)
            {
                var childElement = (Visual)VisualTreeHelper.GetChild(rootElement, i);

                if (childElement is TElement typedChildElement)
                {
                    if (typedChildElement.Name == name)
                    {
                        return typedChildElement;
                    }
                }

                var foundElement = FindElementWithName<TElement>(childElement, name);
                if (foundElement != null)
                {
                    return foundElement;
                }
            }

            return null;
        }

        /// <summary>
        /// Find the framework element for the specified connector.
        /// </summary>
        public static TElement FindElementWithDataContextAndName<TDataContext, TElement>(Visual rootElement, TDataContext data, string name)
            where TDataContext : class
            where TElement : FrameworkElement
        {
            Trace.Assert(rootElement != null);

            if (rootElement is FrameworkElement rootFrameworkElement)
            {
                rootFrameworkElement.UpdateLayout();
            }

            var numChildren = VisualTreeHelper.GetChildrenCount(rootElement);
            for (var i = 0; i < numChildren; ++i)
            {
                var childElement = (Visual)VisualTreeHelper.GetChild(rootElement, i);

                if (childElement is TElement typedChildElement &&
                    typedChildElement.DataContext == data)
                {
                    if (typedChildElement.Name == name)
                    {
                        return typedChildElement;
                    }
                }

                var foundElement = FindElementWithDataContextAndName<TDataContext, TElement>(childElement, data, name);
                if (foundElement != null)
                {
                    return foundElement;
                }
            }

            return null;
        }

        /// <summary>
        /// Find the framework element for the specified connector.
        /// </summary>
        public static TElement FindElementWithType<TElement>(Visual rootElement)
            where TElement : FrameworkElement
        {
            if (rootElement == null)
            {
                throw new ArgumentNullException("rootElement");
            }

            if (rootElement is FrameworkElement rootFrameworkElement)
            {
                rootFrameworkElement.UpdateLayout();
            }

            //
            // Check each child.
            //
            var numChildren = VisualTreeHelper.GetChildrenCount(rootElement);
            for (var i = 0; i < numChildren; ++i)
            {
                var childElement = (Visual)VisualTreeHelper.GetChild(rootElement, i);

                if (childElement is TElement typedChildElement)
                {
                    return typedChildElement;
                }
            }

            //
            // Check sub-trees.
            //
            for (var i = 0; i < numChildren; ++i)
            {
                var childElement = (Visual)VisualTreeHelper.GetChild(rootElement, i);

                var foundElement = FindElementWithType<TElement>(childElement);
                if (foundElement != null)
                {
                    return foundElement;
                }
            }

            return null;
        }

        /// <summary>
        /// Find the framework element for the specified connector.
        /// </summary>
        public static TElement FindElementWithDataContext<TDataContext, TElement>(Visual rootElement, TDataContext data)
            where TDataContext : class
            where TElement : FrameworkElement
        {
            if (rootElement == null)
            {
                throw new ArgumentNullException("rootElement");
            }

            if (rootElement is FrameworkElement rootFrameworkElement)
            {
                rootFrameworkElement.UpdateLayout();
            }

            var numChildren = VisualTreeHelper.GetChildrenCount(rootElement);
            for (var i = 0; i < numChildren; ++i)
            {
                var childElement = (Visual)VisualTreeHelper.GetChild(rootElement, i);

                if (childElement is TElement typedChildElement &&
                    typedChildElement.DataContext == data)
                {
                    return typedChildElement;
                }

                var foundElement = FindElementWithDataContext<TDataContext, TElement>(childElement, data);
                if (foundElement != null)
                {
                    return foundElement;
                }
            }

            return null;
        }

        /// <summary>
        /// Walk up the visual tree and find a template for the specified type.
        /// Returns null if none was found.
        /// </summary>
        public static TDataTemplate FindTemplateForType<TDataTemplate>(Type type, FrameworkElement element)
            where TDataTemplate : class
        {
            var resource = element.TryFindResource(new DataTemplateKey(type));
            if (resource is TDataTemplate dataTemplate)
            {
                return dataTemplate;
            }

            if (type.BaseType != null &&
                type.BaseType != typeof(object))
            {
                dataTemplate = FindTemplateForType<TDataTemplate>(type.BaseType, element);
                if (dataTemplate != null)
                {
                    return dataTemplate;
                }
            }

            foreach (var interfaceType in type.GetInterfaces())
            {
                dataTemplate = FindTemplateForType<TDataTemplate>(interfaceType, element);
                if (dataTemplate != null)
                {
                    return dataTemplate;
                }
            }

            return null;
        }

        /// <summary>
        /// Search the visual tree for template and instance it.
        /// </summary>
        public static FrameworkElement CreateVisual(Type type, FrameworkElement element, object dataContext)
        {
            var template = FindTemplateForType<DataTemplate>(type, element);
            if (template == null)
            {
                throw new ApplicationException("Failed to find DataTemplate for type " + type.Name);
            }

            var visual = (FrameworkElement)template.LoadContent();
            visual.Resources = element.Resources;
            visual.DataContext = dataContext;
            return visual;
        }

        /// <summary>
        /// Layout, measure and arrange the specified element.
        /// </summary>
        public static void InitializeElement(FrameworkElement element)
        {
            element.UpdateLayout();
            element.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
            element.Arrange(new Rect(0, 0, element.DesiredSize.Width, element.DesiredSize.Height));
        }


        /// <summary>
        /// Finds a particular type of UI element int he visual tree that has the specified data context.
        /// </summary>
        public static ICollection<T> FindTypedElements<T>(DependencyObject rootElement) where T : DependencyObject
        {
            var foundElements = new List<T>();
            FindTypedElements(rootElement, foundElements);
            return foundElements;
        }

        /// <summary>
        /// Finds a particular type of UI element int he visual tree that has the specified data context.
        /// </summary>
        private static void FindTypedElements<T>(DependencyObject rootElement, List<T> foundElements) where T : DependencyObject
        {
            var numChildren = VisualTreeHelper.GetChildrenCount(rootElement);
            for (var i = 0; i < numChildren; ++i)
            {
                var childElement = VisualTreeHelper.GetChild(rootElement, i);
                if (childElement is T element)
                {
                    foundElements.Add(element);
                    continue;
                }

                FindTypedElements<T>(childElement, foundElements);
            }
        }

        /// <summary>
        /// Recursively dump out all elements in the visual tree.
        /// </summary>
        public static void DumpVisualTree(Visual root)
        {
            DumpVisualTree(root, 0);
        }

        /// <summary>
        /// Recursively dump out all elements in the visual tree.
        /// </summary>
        private static void DumpVisualTree(Visual root, int indentLevel)
        {
            var indentStr = new string(' ', indentLevel * 2);
            Trace.Write(indentStr);
            Trace.Write(root.GetType().Name);

            if (root is FrameworkElement rootElement)
            {
                if (rootElement.DataContext != null)
                {
                    Trace.Write(" [");
                    Trace.Write(rootElement.DataContext.GetType().Name);
                    Trace.Write("]");
                }
            }

            Trace.WriteLine("");

            var numChildren = VisualTreeHelper.GetChildrenCount(root);
            if (numChildren > 0)
            {
                Trace.Write(indentStr);
                Trace.WriteLine("{");

                for (var i = 0; i < numChildren; ++i)
                {
                    var child = (Visual)VisualTreeHelper.GetChild(root, i);
                    DumpVisualTree(child, indentLevel + 1);
                }

                Trace.Write(indentStr);
                Trace.WriteLine("}");
            }
        }
    }
}
