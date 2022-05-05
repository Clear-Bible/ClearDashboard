
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ClearDashboard.DataAccessLayer.Annotations;
using ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.DataAccessLayer.ViewModels;

public class ParatextProjectViewModel : ParatextProject, INotifyPropertyChanged
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

    public event PropertyChangedEventHandler? PropertyChanged;

    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}