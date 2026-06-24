using Core;

namespace Web;

public class StatusCheckScheduler : BackgroundService
{
    private readonly IStatusCheckService _statusCheckService;
    private readonly IUserSettingsRepository _settingsRepository;
    private readonly ILogger<StatusCheckScheduler> _logger;

    public StatusCheckScheduler(
        IStatusCheckService statusCheckService,
        IUserSettingsRepository settingsRepository,
        ILogger<StatusCheckScheduler> logger)
    {
        _statusCheckService = statusCheckService;
        _settingsRepository = settingsRepository;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Initial delay - let app start before first check
        await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await _statusCheckService.CheckAllAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Status check failed.");
            }

            var settings = _settingsRepository.Get();
            var interval = TimeSpan.FromHours(settings.StatusCheckIntervalHours);
            _logger.LogInformation("Next status check in {interval}", interval);
            await Task.Delay(interval, stoppingToken);
        }
    }
}