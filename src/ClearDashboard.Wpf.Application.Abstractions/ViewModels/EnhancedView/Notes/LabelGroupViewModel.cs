using Caliburn.Micro;
using ClearDashboard.DAL.Alignment.Notes;
using ClearDashboard.Wpf.Application.Collections.Notes;

namespace ClearDashboard.Wpf.Application.ViewModels.EnhancedView.Notes
{
    public class LabelGroupViewModel : PropertyChangedBase
    {
        public LabelGroup Entity { get; }
        public LabelGroupId? LabelGroupId => Entity.LabelGroupId;
        public LabelCollection Labels { get; set; } = new();

        public string? Name
        {
            get => Entity.Name ?? string.Empty;
            set
            {
                if (Equals(value, Entity.Name)) return;
                Entity.Name = value;
                NotifyOfPropertyChange();
            }
        }

        public LabelGroupViewModel()
        {
            Entity = new LabelGroup();
        }

        public LabelGroupViewModel(LabelGroup labelGroup)
        {
            Entity = labelGroup;
        }
    }
}
