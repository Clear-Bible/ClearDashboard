using System.Windows;
using System.Windows.Controls;
using AvalonDock.Layout;
using ViewModels.Project;

namespace ClearDashboard.Wpf.Views.Panes;

internal class ProjectWorkspacePanesTemplateSelector : DataTemplateSelector
{

    // ====================
    //   DOCUMENTS
    // ====================
    public DataTemplate AlignmentViewDataTemplate
    {
        get;
        set;
    }

    public DataTemplate CorpusViewDataTemplate
    {
        get;
        set;
    }

    // ====================
    //        TOOLS
    // ====================
    public DataTemplate ProjectDesignSurfaceDataTemplate
    {
        get;
        set;
    }

    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
        var itemAsLayoutContent = item as LayoutContent;

        // ====================
        //   DOCUMENTS
        // ====================
        if (item is CorpusViewModel)
            return CorpusViewDataTemplate;

        if (item is AlignmentViewModel)
            return AlignmentViewDataTemplate;



        // ====================
        //        TOOLS
        // ====================
        if (item is ProjectDesignSurfaceViewModel)
        {
            return ProjectDesignSurfaceDataTemplate;
        }

        return base.SelectTemplate(item, container);
    }
}