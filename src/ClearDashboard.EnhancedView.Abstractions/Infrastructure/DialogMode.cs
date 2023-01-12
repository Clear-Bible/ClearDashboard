namespace ClearDashboard.DataAccessLayer.Wpf.Infrastructure;

public enum DialogMode
{
    Add,
    Edit
}

public interface IDialog
{
    DialogMode DialogMode { get; set; }
}