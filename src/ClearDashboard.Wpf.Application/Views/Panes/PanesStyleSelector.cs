using System.Windows;
using System.Windows.Controls;
using ClearDashboard.Wpf.Application.ViewModels;
using ClearDashboard.Wpf.Application.ViewModels.Corpus;
using ClearDashboard.Wpf.Application.ViewModels.ParatextViews;
using ClearDashboard.Wpf.Application.ViewModels.Project;

namespace ClearDashboard.Wpf.Application.Views.Panes
{
    public class PanesStyleSelector : StyleSelector
    {

        #region Pages - Documents
        // ====================
        //        DOCUMENTS
        // ====================
        public Style DocumentStyle
        {
            get;
            set;
        }

        #endregion


        #region Tools
        // ====================
        //        TOOLS
        // ====================
        public Style ToolStyle
        {
            get;
            set;
        }



        #endregion


        public override System.Windows.Style SelectStyle(object item, System.Windows.DependencyObject container)
        {
            // ====================
            //   DOCUMENTS
            // ====================
            //if (item is StartPageViewModel)
            //{
            //    return DocumentStyle;
            //}

            if (item is AlignmentToolViewModel)
            {
                return DocumentStyle;
            }

            if (item is EnhancedCorpusViewModel)
            {
                return DocumentStyle;
            }

            if (item is DashboardViewModel)
            {
                return DocumentStyle;
            }

            if (item is CorpusTokensViewModel)
            {
                return DocumentStyle;
            }

            //if (item is TreeDownViewModel)
            //{
            //    return DocumentStyle;
            //}

            //if (item is ConcordanceViewModel)
            //{
            //    return DocumentStyle;
            //}



            // ====================
            //   TOOLS
            // ====================
            if (item is BiblicalTermsViewModel)
            {
                return ToolStyle;
            }

            if (item is WordMeaningsViewModel)
            {
                return ToolStyle;
            }

            //if (item is SourceContextViewModel)
            //{
            //    return ToolStyle;
            //}

            //if (item is TargetContextViewModel)
            //{
            //    return ToolStyle;
            //}

            //if (item is NotesViewModel)
            //{
            //    return ToolStyle;
            //}

            if (item is PinsViewModel)
            {
                return ToolStyle;
            }

            if (item is TextCollectionsViewModel)
            {
                return ToolStyle;
            }

            return base.SelectStyle(item, container);
        }
    }
}
