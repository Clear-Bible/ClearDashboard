using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using ClearDashboard.Wpf.ViewModels;

namespace ClearDashboard.Wpf.UserControls
{
    using AvalonDock.Layout;

    class PanesTemplateSelector : DataTemplateSelector
    {
        public PanesTemplateSelector()
        {

        }

        // ====================
        //   DOCUMENTS
        // ====================
        public DataTemplate StartPageViewTemplate
        {
            get;
            set;
        }

        public DataTemplate AlignmentToolViewTemplate
        {
            get;
            set;
        }

        public DataTemplate TreeDownViewTemplate
        {
            get;
            set;
        }

        // ====================
        //        TOOLS
        // ====================
        public DataTemplate BiblicalTermsViewTemplate
        {
            get;
            set;
        }

        public DataTemplate WordMeaningsViewTemplate
        {
            get;
            set;
        }

        public DataTemplate SourceContextViewTemplate
        {
            get;
            set;
        }

        public DataTemplate TargetContextViewTemplate
        {
            get;
            set;
        }

        public DataTemplate NotesViewTemplate
        {
            get;
            set;
        }

        public DataTemplate PinsViewTemplate
        {
            get;
            set;
        }

        public DataTemplate TextCollectionViewTemplate
        {
            get;
            set;
        }

        public override System.Windows.DataTemplate SelectTemplate(object item, System.Windows.DependencyObject container)
        {
            var itemAsLayoutContent = item as LayoutContent;

            // ====================
            //   DOCUMENTS
            // ====================
            if (item is StartPageViewModel)
                return StartPageViewTemplate;

            if (item is AlignmentToolViewModel)
                return AlignmentToolViewTemplate;


            if (item is TreeDownViewModel)
                return TreeDownViewTemplate;

            // ====================
            //        TOOLS
            // ====================
            if (item is BiblicalTermsViewModel)
            {
                return BiblicalTermsViewTemplate;
            }

            if (item is WordMeaningsViewModel)
            {
                return WordMeaningsViewTemplate;
            }

            if (item is SourceContextViewModel)
            {
                return SourceContextViewTemplate;
            }

            if (item is TargetContextViewModel)
            {
                return TargetContextViewTemplate;
            }

            if (item is NotesViewModel)
            {
                return NotesViewTemplate;
            }

            if (item is PinsViewModel)
            {
                return PinsViewTemplate;
            }

            if (item is TextCollectionViewModel)
            {
                return TextCollectionViewTemplate;
            }

            return base.SelectTemplate(item, container);
        }
    }
}
