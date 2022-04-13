using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.DataAccessLayer.BackgroundServices;

public class ClearEngineProcessingService : IClearEngineProcessingService
{
    private int _executionCount;
    private readonly ILogger<ClearEngineProcessingService> _logger;

    public ClearEngineProcessingService(
        ILogger<ClearEngineProcessingService> logger) =>
        _logger = logger;

    public async Task DoWorkAsync(CancellationToken stoppingToken)
    {
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                ++_executionCount;

                _logger.LogInformation(
                    "{ServiceName} working, execution count: {Count}",
                    nameof(ClearEngineProcessingService),
                    _executionCount);

                await Task.Delay(50, stoppingToken);
            }
        }
        catch (Exception)
        {
            _logger.LogInformation("{ServiceName} stopped.", nameof(ClearEngineProcessingService));
            throw;
        }
            
    }
}