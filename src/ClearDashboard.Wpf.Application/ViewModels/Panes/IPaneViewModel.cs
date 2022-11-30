using System;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;

namespace ClearDashboard.Wpf.Application.ViewModels.Panes;

public interface IPaneViewModel
{
    Guid PaneId { get; }
    ICommand RequestCloseCommand { get; set; }
    //string Title { get; set; }
    ImageSource? IconSource { get; }
    bool IsSelected { get; set; }
    string ContentId { get; set; }
    //new bool IsActive { get; set; }
    Task RequestClose(object obj);
}