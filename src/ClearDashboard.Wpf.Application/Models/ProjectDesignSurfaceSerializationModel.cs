using ClearDashboard.Wpf.Controls.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.Wpf.Application.ViewModels;

namespace ClearDashboard.Wpf.Application.Models
{
    public class ProjectDesignSurfaceSerializationModel
    {
        public ImpObservableCollection<SerializedConnections> Connections { get; set; } = new();
        public ImpObservableCollection<SerializedNodes> CorpusNodes { get; set; } = new();
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
}
