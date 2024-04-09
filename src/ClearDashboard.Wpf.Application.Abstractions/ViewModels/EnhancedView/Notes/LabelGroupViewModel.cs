using System.Collections.Specialized;
using System.Linq;
using Caliburn.Micro;
using ClearDashboard.DAL.Alignment.Notes;
using ClearDashboard.Wpf.Application.Collections.Notes;

namespace ClearDashboard.Wpf.Application.ViewModels.EnhancedView.Notes
{

    public class LabelViewModel : PropertyChangedBase
    {
        private bool _selected;

        public LabelViewModel(Label entity)
        {
            Entity = entity;
        }

        public Label Entity { get; }

        public string? Text => Entity.Text ?? string.Empty;

        public bool Selected
        {
            get => _selected;
            set => Set(ref _selected, value);
        }
    }
    public class LabelGroupViewModel : PropertyChangedBase
    {
        public LabelGroup Entity { get; }
        public LabelGroupId? LabelGroupId => Entity.LabelGroupId;
        public bool IsNoneLabelGroup => LabelGroupId == null;
        public LabelCollection Labels { get; set; } = new();

        public BindableCollection<LabelViewModel> SelectableLabels { get; set; } = new();

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

        
            Labels.CollectionChanged += LabelCollectionChanged;
        }

        private void LabelCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                var labels = e.NewItems.Cast<Label>().ToList();
                SelectableLabels.AddRange(labels.Select(i => new LabelViewModel(i)));
            }
        }

        public LabelGroupViewModel(LabelGroup labelGroup)
        {
            Entity = labelGroup;
        }

        public void InitializeSelectableLabels()
        {
            SelectableLabels.Clear();
            SelectableLabels.AddRange(Labels.Select(i => new LabelViewModel(i)));
        }
    }
}
