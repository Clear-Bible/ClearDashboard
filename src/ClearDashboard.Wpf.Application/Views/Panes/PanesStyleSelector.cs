using ClearDashboard.Wpf.Application.ViewModels;
using ClearDashboard.Wpf.Application.ViewModels.ParatextViews;
using System.Windows;
using System.Windows.Controls;
using ClearDashboard.Wpf.Application.ViewModels.Marble;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView;
using ClearDashboard.Wpf.Application.ViewModels.Lexicon;

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

            if (item is EnhancedViewModel)
            {
                return DocumentStyle;
            }


            // ====================
            //   TOOLS
            // ====================
            if (item is BiblicalTermsViewModel)
            {
                return ToolStyle;
            }

            //if (item is WordMeaningsViewModel)
            //{
            //    return ToolStyle;
            //}

            if (item is MarbleViewModel)
            {
                return ToolStyle;
            }

            if (item is PinsViewModel)
            {
                return ToolStyle;
            }

            if (item is LexiconViewModel)
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
