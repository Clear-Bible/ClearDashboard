using MvvmHelpers;

namespace ParaTextPlugin.Data.Models
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