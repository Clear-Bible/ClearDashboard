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


        public DataTemplate BiblicalTermsViewTemplate
        {
            get;
            set;
        }

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

        public override System.Windows.DataTemplate SelectTemplate(object item, System.Windows.DependencyObject container)
        {
            var itemAsLayoutContent = item as LayoutContent;

            if (item is BiblicalTermsViewModel)
            {
                return BiblicalTermsViewTemplate;
            }

            if (item is StartPageViewModel)
                return StartPageViewTemplate;

            if (item is AlignmentToolViewModel)
                return AlignmentToolViewTemplate;


            if (item is TreeDownViewModel)
                return TreeDownViewTemplate;

            return base.SelectTemplate(item, container);
        }
    }
}
