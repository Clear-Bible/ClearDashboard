using AvalonDock.Layout;
using System.Linq;

namespace ClearDashboard.Wpf.Application.Views.Panes
{
    public class LayoutInitializer : ILayoutUpdateStrategy
    {
        public bool BeforeInsertAnchorable(LayoutRoot layout, LayoutAnchorable anchorableToShow, ILayoutContainer destinationContainer)
        {

            // Lock tool windows in their default layout position
            //anchorableToShow.CanMove = false;
            //anchorableToShow.CanHide = false;
            //anchorableToShow.CanFloat = false;
            //anchorableToShow.CanAutoHide = false;

            //AD wants to add the anchorable into destinationContainer
            //just for test provide a new anchorablepane 
            //if the pane is floating let the manager go ahead
            LayoutAnchorablePane destPane = destinationContainer as LayoutAnchorablePane;
            if (destinationContainer != null &&
                destinationContainer.FindParent<LayoutFloatingWindow>() != null)
                return false;

            var toolsPane = layout.Descendents().OfType<LayoutAnchorablePane>().FirstOrDefault(d => d.Name == "ToolsPane");
            if (toolsPane != null)
            {
                toolsPane.Children.Add(anchorableToShow);
                return true;
            }



            return false;

        }


        public void AfterInsertAnchorable(LayoutRoot layout, LayoutAnchorable anchorableShown)
        {
        }


        public bool BeforeInsertDocument(LayoutRoot layout, LayoutDocument anchorableToShow, ILayoutContainer destinationContainer)
        {
            //anchorableToShow.CanMove = false;
            //anchorableToShow.CanFloat = false;
            //anchorableToShow.CanClose = false;
            return false;
        }

        public void AfterInsertDocument(LayoutRoot layout, LayoutDocument anchorableShown)
        {

        }
    }
}
