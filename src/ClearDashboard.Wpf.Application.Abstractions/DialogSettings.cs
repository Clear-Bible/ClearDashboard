using System.Dynamic;
using System.Windows;

namespace ClearDashboard.DataAccessLayer.Wpf;

public static class DialogSettings
{
    public static dynamic NewProjectDialogSettings => CreateNewProjectDialogSettings();
    public static dynamic AddParatextCorpusDialogSettings => CreateAddParatextCorpusDialogSettings();
    public static dynamic AddNewInterlinearDialogSettings => CreateAddNewInterlinearDialogSettings();

    private static dynamic CreateNewProjectDialogSettings()
    {
        dynamic settings = new ExpandoObject();
        settings.WindowStyle = WindowStyle.None;
        settings.ShowInTaskbar = false;
        settings.WindowState = WindowState.Normal;
        settings.ResizeMode = ResizeMode.NoResize;

        // Keep the window on top
        //settings.Topmost = true;
        settings.Owner = System.Windows.Application.Current.MainWindow;

        return settings;
    }

    private static dynamic CreateAddParatextCorpusDialogSettings()
    {
        dynamic settings = new ExpandoObject();
        settings.WindowStyle = WindowStyle.None;
        settings.ShowInTaskbar = false;
        settings.WindowState = WindowState.Normal;
        settings.ResizeMode = ResizeMode.NoResize;
        settings.Width = 850;
        settings.Height = 600;

        // Keep the window on top
        //settings.Topmost = true;
        settings.Owner = System.Windows.Application.Current.MainWindow;
        return settings;
    }

    private static dynamic CreateAddNewInterlinearDialogSettings()
    {
        dynamic settings = new ExpandoObject();
        settings.WindowStyle = WindowStyle.None;
        settings.ShowInTaskbar = false;
        settings.WindowState = WindowState.Normal;
        settings.ResizeMode = ResizeMode.NoResize;
        settings.Width = 850;
        settings.Height = 600;

        // Keep the window on top
        //settings.Topmost = true;
        settings.Owner = System.Windows.Application.Current.MainWindow;

        return settings;
    }

}