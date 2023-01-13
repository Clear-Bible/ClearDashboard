namespace ClearDashboard.Wpf.Application.Infrastructure;

public enum DialogMode
{
    Add,
    Edit
}

public interface IDialog
{
    DialogMode DialogMode { get; set; }
}