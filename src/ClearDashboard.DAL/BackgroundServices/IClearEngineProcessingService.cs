using System.Threading;
using System.Threading.Tasks;

namespace ClearDashboard.DataAccessLayer.BackgroundServices;

public interface IClearEngineProcessingService
{
    Task DoWorkAsync(CancellationToken stoppingToken);
}