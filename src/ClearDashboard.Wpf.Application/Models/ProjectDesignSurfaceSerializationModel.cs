using ClearDashboard.Wpf.Controls.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.Wpf.Application.ViewModels;
using System.Collections.ObjectModel;
using ClearDashboard.DAL.Alignment.Corpora;

namespace ClearDashboard.Wpf.Application.Models
{
    public class ProjectDesignSurfaceSerializationModel
    {
        public ImpObservableCollection<SerializedConnections> Connections { get; set; } = new();
        public ImpObservableCollection<SerializedNodes> CorpusNodes { get; set; } = new();

        public ObservableCollection<SerializedCopora> Corpora = new();
    }

    public class SerializedNodes
    {
        public string ParatextProjectId = string.Empty;
        public string Name { get; set; } = string.Empty;
        public double X { get; set; }
        public double Y { get; set; }
        public CorpusType CorpusType = CorpusType.Standard;
        public List<NodeTokenization> NodeTokenizations = new();
    }

    public class SerializedConnections
    {
        public string SourceConnectorId { get; set; }
        public string TargetConnectorId { get; set; }
    }

    public class SerializedCopora
    {
        public string CorpusId { get; set; }
        public bool IsRtl { get; set; }
        public string? Name { get; set; }
        public string? DisplayName { get; set; }
        public string? Language { get; set; }
        public string? ParatextGuid { get; set; }
        public string CorpusType { get; set; }
        public DateTimeOffset? Created { get; set; }
        public string? UserId { get; set; }
    }

}
