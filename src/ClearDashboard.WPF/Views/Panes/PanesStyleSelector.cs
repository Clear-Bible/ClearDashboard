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
        public Style StartPageStyle
        {
            get;
            set;
        }

        public Style AlignmentPageStyle
        {
            get;
            set;
        }

        public Style TreeDownPageStyle
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

        public Style FileStyle
        {
            get;
            set;
        }

        #endregion


        public override System.Windows.Style SelectStyle(object item, System.Windows.DependencyObject container)
        {
            if (item is ToolViewModel)
                return ToolStyle;

            if (item is FileViewModel)
                return FileStyle;

            if (item is StartPageViewModel)
                return StartPageStyle;
            
            if(item is AlignmentToolViewModel)
                return AlignmentPageStyle;

            if (item is TreeDownViewModel)
                return TreeDownPageStyle;

            return base.SelectStyle(item, container);
        }
    }
}
