using MvvmHelpers;

namespace ClearDashboard.Pipes_Shared.Models
{
    public class AssignedUser : ObservableObject
    {
        private string _name;
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }
    }
}