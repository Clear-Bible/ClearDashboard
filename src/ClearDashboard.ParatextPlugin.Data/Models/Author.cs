using MvvmHelpers;

namespace ClearDashboard.ParatextPlugin.Data.Models
{
    public class Author : ObservableObject
    {
        private string _name;
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

    }
}
