# Performance Mode How-To

This serves as a guide as to how to add the ability for Dashboard to kick into High Performance mode.

## Settings

In the Dashboard Settings Window, there is a checkbox to enable/disable this setting.  Note that if the
system detects that you are not on a laptop, this checkbox will be disabled. This window is linked to the 
setting in the `System.Default.EnablePowerModes` in the ClearDashboard.Wpf.Application's Settings.settings file.

## How to use

In the module that you intend to use this feature, inject the following into the constructor and link it to the 
private variable:

```
#region Member Variables   

    private readonly BackgroundTasksViewModel _backgroundTasksViewModel;
    private readonly SystemPowerModes _systemPowerModes = new();

#endregion //Member Variables


#region Constructor

    public ProjectDesignSurfaceViewModel(... BackgroundTasksViewModel backgroundTasksViewModel, ...)
        : base(...)
    {
        _backgroundTasksViewModel = backgroundTasksViewModel;
    }

#endregion //Constructor

```

Now with the variable to SystemPowerModes initialized, you can turn on the performance mode with the following commands:

``` c#

    // check to see if we want to run this in High Performance mode
    if (Settings.Default.EnablePowerModes && _systemPowerModes.IsLaptop)
    {
        await _systemPowerModes.TurnOnHighPerformanceMode();
    }

```

The above function will check to make sure that the user's machine is a laptop and that he desires to have this extra
functionality enabled. The `_systemPowerModes.TurnOnHighPerformanceMode()` will look for a default High Performance mode
on the laptop. If it doesn't find one, it will restore it and then turn it on.

If there are multiple processes running in parallel (e.g., tokienization of corpus), then you will need to set the background message
to include that the longrunning background tasks are in performance mode `backgroundTaskTypeEnum`:

```
    await SendBackgroundStatus(taskName, LongRunningTaskStatus.Running,
        description: $"Creating corpus '{selectedProject.Name}'...", cancellationToken: cancellationToken, 
        backgroundTaskTypeEnum: BackgroundTaskTypeEnum.PerformanceMode);
```

This will allow many tasks to run under the perfomance mode and won't disable it until after there are no more of these
types of background tasks remaining.

To restore the previous power mode on the laptop (e.g., Power Saver), run the following:

``` c#

    // check to see if there are still High Performance Tasks still out there
    var numTasks = _backgroundTasksViewModel.GetNumberOfPerformanceTasksRemaining();
    if (numTasks == 0 && _systemPowerModes.IsHighPerformanceEnabled)
    {
        // shut down high performance mode
        await _systemPowerModes.TurnOffHighPerformanceMode();
    }
```