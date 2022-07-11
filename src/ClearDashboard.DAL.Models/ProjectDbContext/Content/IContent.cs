namespace ClearDashboard.DataAccessLayer.Models;

public interface IContent<T>
{
    T Content { get; set; }
}