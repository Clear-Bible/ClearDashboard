using System.Collections.ObjectModel;
using ClearDashboard.Wpf.Controls.Utils;

namespace ClearDashboard.Wpf.Application.Models.ProjectSerialization
{
    public class ProjectDesignSurfaceSerializationModel
    {
        public ImpObservableCollection<SerializedConnection> Connections { get; set; } = new();
        public ImpObservableCollection<SerializedNode> CorpusNodes { get; set; } = new();

        public ObservableCollection<SerializedCorpus> Corpora = new();
    }
}
