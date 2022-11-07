using Caliburn.Micro;
using ClearBible.Engine.Utils;
using ClearDashboard.DAL.Alignment;

namespace ClearDashboard.Wpf.Application.ViewModels.Display
{
    public class NoteAssociationViewModel : PropertyChangedBase
    {
        private string _description = string.Empty;

        public IId AssociatedEntityId { get; set; } = new EmptyEntityId();

        public string Description
        {
            get => _description;
            set
            {
                if (value == _description) return;
                _description = value;
                NotifyOfPropertyChange();
            }
        }
    }
}
