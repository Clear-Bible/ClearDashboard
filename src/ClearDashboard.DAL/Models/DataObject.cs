using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ClearDashboard.DAL.Models
{
    /// <summary>
    /// Abstract class to be inherited by data object classes so that the CRUD statements can be called for any data object in a uniform manner.
    /// The property names should be the same as the fields in the DB. When types need to be converted use a TypeHandler or create another 
    /// property that reads and converts the property for storage in the DB. The other property would be used int the application.
    /// 
    /// To write the SQL and create tables I use the freeware https://sqlitebrowser.org/
    /// 
    /// This base class implements INotifyPropertyChanged, so that classes inheriting this class don't have to. When you create properties
    /// in your class that need to trigger a binding to a UI control, make sure to use the full property and on the setter call OnPropertyChanged();
    /// </summary>
    public abstract class DataObject : INotifyPropertyChanged
    {
        /// <summary>
        /// Every object should have an Id. Let the db engine set the value.
        /// You can check if the record has been saved by testing to see if Id == -1
        /// </summary>
        public virtual int Id { get; private set; } = -1;

        /// <summary>
        /// Triggers an update of the UI when the data changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
