using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClearDashboard.DataAccessLayer.Wpf.Messages
{

    public record NodeSelectedChangedMessage(object? Node);
    public record ConnectionSelectedChangedMessage(Guid ConnectorId);
    public record CorpusAddedMessage(string ParatextId);
    public record CorpusDeletedMessage(string ParatextId);
    public record CorpusSelectedMessage(string ParatextId);
    public record CorpusDeselectedMessage(string ParatextId);
    public record ParallelCorpusAddedMessage(string SourceParatextId, string TargetParatextId, Guid ConnectorGuid);
    public record ParallelCorpusDeletedMessage(string SourceParatextId, string TargetParatextId, Guid ConnectorGuid);
    public record ParallelCorpusSelectedMessage(string SourceParatextId, string TargetParatextId, Guid ConnectorGuid);
    public record ParallelCorpusDeselectedMessage(Guid ConnectorGuid);

}
