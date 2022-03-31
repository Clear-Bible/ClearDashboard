using ClearDashboard.Common.Models;

namespace ClearDashboard.DataAccessLayer.ViewModels;

public class ParatextProjectViewModel : ParatextProject
{
    private bool _inUse;
    public bool InUse
    {
        get => _inUse;
        set
        {
            _inUse = value;
            OnPropertyChanged(nameof(InUse));
        }
    }
}