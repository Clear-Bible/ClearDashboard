using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClearDashboard.Wpf.ViewModels;

namespace ClearDashboard.Wpf.UserControls
{
    using System.Windows.Controls;
    using System.Windows;

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
            if (item is StartPageViewModel)
                return DocumentStyle;

            if (item is AlignmentToolViewModel)
                return DocumentStyle;

            if (item is TreeDownViewModel)
                return DocumentStyle;


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

            if (item is SourceContextViewModel)
            {
                return ToolStyle;
            }

            if (item is TargetContextViewModel)
            {
                return ToolStyle;
            }

            if (item is NotesViewModel)
            {
                return ToolStyle;
            }

            if (item is PinsViewModel)
            {
                return ToolStyle;
            }

            if (item is TextCollectionViewModel)
            {
                return ToolStyle;
            }

            return base.SelectStyle(item, container);
        }
    }
}
