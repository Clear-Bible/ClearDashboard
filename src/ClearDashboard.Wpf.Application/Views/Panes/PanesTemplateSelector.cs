using ClearDashboard.Wpf.Application.ViewModels;
using ClearDashboard.Wpf.Application.ViewModels.ParatextViews;
using ClearDashboard.Wpf.Application.ViewModels.Project;
using System.Windows;
using System.Windows.Controls;

namespace ClearDashboard.Wpf.Application.Views.Panes
{
    public class PanesTemplateSelector : DataTemplateSelector
    {
        public PanesTemplateSelector()
        {
            // no-op
        }

        // ====================
        //   DOCUMENTS
        // ====================

        public DataTemplate EnhancedCorpusViewTemplate
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

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            // ====================
            //   DOCUMENTS
            // ====================

            if(item is EnhancedCorpusViewModel)
                return EnhancedCorpusViewTemplate;

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
