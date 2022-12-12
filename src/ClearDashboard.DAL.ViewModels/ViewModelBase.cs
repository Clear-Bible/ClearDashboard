using System.Text.Json.Serialization;
using Caliburn.Micro;

namespace ClearDashboard.DAL.ViewModels;

public abstract class ViewModelBase<T> : PropertyChangedBase where T: class, new()
{
    protected ViewModelBase() : this(new T()) {}

    protected ViewModelBase(T entity)
    {
        Entity = entity;
    }
    [JsonIgnore]
    public T Entity { get; private set; }
}