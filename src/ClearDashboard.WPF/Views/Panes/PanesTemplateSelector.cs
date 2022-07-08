using System.Windows;
using System.Windows.Controls;
using AvalonDock.Layout;
using ClearDashboard.Wpf.ViewModels;

namespace ClearDashboard.Wpf.Views.Panes
{
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

        public DataTemplate DashboardViewTemplate
        {
            get;
            set;
        }

        public DataTemplate ConcordanceViewTemplate
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

            if (item is DashboardViewModel)
                return DashboardViewTemplate;

            if (item is ConcordanceViewModel)
                return ConcordanceViewTemplate;

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

            if (item is TextCollectionsViewModel)
            {
                return TextCollectionViewTemplate;
            }

            return base.SelectTemplate(item, container);
        }
    }
}
