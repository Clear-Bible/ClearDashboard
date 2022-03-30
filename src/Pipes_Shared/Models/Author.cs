using MvvmHelpers;

namespace ClearDashboard.Pipes_Shared.Models
{
    public class Author : ObservableObject
    {
        private string _Name;
        public string Name
        {
            get => _Name;
            set { SetProperty(ref _Name, value); }
        }

    }
}
