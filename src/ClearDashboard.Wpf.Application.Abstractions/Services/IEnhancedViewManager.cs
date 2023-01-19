using ClearDashboard.Wpf.Application.Models.EnhancedView;
using System.Threading;
using System.Threading.Tasks;

namespace ClearDashboard.Wpf.Application.Services
{
    public interface IEnhancedViewManager
    {
        Task AddMetadatumEnhancedView(EnhancedViewItemMetadatum metadatum, CancellationToken cancellationToken);
    }
}
