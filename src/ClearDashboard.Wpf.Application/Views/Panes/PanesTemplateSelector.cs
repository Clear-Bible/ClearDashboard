using ClearDashboard.Wpf.Application.ViewModels;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView;
using ClearDashboard.Wpf.Application.ViewModels.Marble;
using ClearDashboard.Wpf.Application.ViewModels.Notes;
using ClearDashboard.Wpf.Application.ViewModels.ParatextViews;
using System.Windows;
using System.Windows.Controls;
using ClearDashboard.Wpf.Application.ViewModels.Lexicon;

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

        public DataTemplate LexiconViewTemplate
        {
            get;
            set;
        }

        public DataTemplate WordMeaningsViewTemplate
        {
            get;
            set;
        }
        
        public DataTemplate MarbleViewTemplate
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

        public DataTemplate NotesViewTemplate
        {
            get;
            set;
        }
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            // ====================
            //   DOCUMENTS
            // ====================

            if(item is EnhancedViewModel)
                return EnhancedCorpusViewTemplate;

            // ====================
            //        TOOLS
            // ====================
            if (item is BiblicalTermsViewModel)
            {
                return BiblicalTermsViewTemplate;
            }

            if (item is LexiconViewModel)
            {
                return LexiconViewTemplate;
            }

            //if (item is WordMeaningsViewModel)
            //{
            //    return WordMeaningsViewTemplate;
            //}

            if (item is MarbleViewModel)
            {
                return MarbleViewTemplate;
            }


            if (item is PinsViewModel)
            {
                return PinsViewTemplate;
            }

            if (item is TextCollectionsViewModel)
            {
                return TextCollectionViewTemplate;
            }

            if (item is NotesViewModel)
            {
                return NotesViewTemplate;
            }

            return base.SelectTemplate(item, container);
        }
    }
}
