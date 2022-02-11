using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ClearDashboard.Wpf.UserControls
{
    using AvalonDock.Layout;

    class PanesTemplateSelector : DataTemplateSelector
    {
        public PanesTemplateSelector()
        {

        }


        public DataTemplate FileViewTemplate
        {
            get;
            set;
        }

        public DataTemplate FileStatsViewTemplate
        {
            get;
            set;
        }

        public override System.Windows.DataTemplate SelectTemplate(object item, System.Windows.DependencyObject container)
        {
            var itemAsLayoutContent = item as LayoutContent;

            //if (item is FileViewModel)
            //    return FileViewTemplate;

            //if (item is FileStatsViewModel)
            //    return FileStatsViewTemplate;

            return base.SelectTemplate(item, container);
        }
    }
}
