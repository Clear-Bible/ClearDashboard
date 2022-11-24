using System.Collections.ObjectModel;
using ClearDashboard.Wpf.Controls.Utils;

namespace ClearDashboard.Wpf.Application.Models.ProjectSerialization
{
    public class ProjectDesignSurfaceSerializationModel
    {
        public ImpObservableCollection<SerializedParallelCorpus> ParallelCorpora { get; set; } = new();
        public ImpObservableCollection<SerializedTokenizedCorpus> TokenizedCorpora { get; set; } = new();

        //public ObservableCollection<SerializedCorpus> Corpora { get; set; }  = new();
    }
}
