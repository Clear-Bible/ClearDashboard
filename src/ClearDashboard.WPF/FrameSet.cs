using System.Windows.Controls;
using System.Windows.Navigation;
using Caliburn.Micro;

namespace ClearDashboard.Wpf;

public class FrameSet
{
    public FrameSet()
    {
        Frame = new Frame
        {
            NavigationUIVisibility = NavigationUIVisibility.Hidden
        };
        FrameAdapter = new FrameAdapter(Frame);
    }
    public Frame Frame { get; private set; }
    public FrameAdapter FrameAdapter { get; private set; }
    public INavigationService NavigationService => FrameAdapter as INavigationService;
}