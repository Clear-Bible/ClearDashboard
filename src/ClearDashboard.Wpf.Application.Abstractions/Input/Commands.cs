using System.Windows.Input;

namespace ClearDashboard.Wpf.Application.Input;

public static class Commands
{
    public static readonly ICommand FormatDocument;
    public static readonly ICommand FormatSelection;

    //this command is super secret - I can't believe you found it
    public static readonly ICommand Secret = new RoutedUICommand();

    static Commands()
    {
        var inputGestures = new InputGestureCollection { new MultiKeyGesture(new Key[] { Key.K, Key.D }, ModifierKeys.Control, "Ctrl+K, D") };
        FormatDocument = new RoutedUICommand("Format Document", "FormatDocument", typeof(Commands), inputGestures);

        inputGestures = new InputGestureCollection { new MultiKeyGesture(new Key[] { Key.K, Key.F }, ModifierKeys.Control, "Ctrl+K, F") };
        FormatSelection = new RoutedUICommand("Format Selection", "FormatSelection", typeof(Commands), inputGestures);
    }
}