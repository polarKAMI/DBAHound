using Core;

namespace Web;

public class StatusCheckService : IStatusCheckService
{
    private readonly IListingRepository _listingRepository;
    private readonly IMatchResultRepository _matchResultRepository;
    private readonly ILogger<StatusCheckService> _logger;
    private const int DelayMs = 500;

    public StatusCheckService(
        IListingRepository listingRepository,
        IMatchResultRepository matchResultRepository,
        ILogger<StatusCheckService> logger)
    {
        _listingRepository = listingRepository;
        _matchResultRepository = matchResultRepository;
        _logger = logger;
    }

    public async Task CheckAllAsync(CancellationToken cancellationToken)
    {
        var matches = _matchResultRepository.GetAll()
            .Where(m => !m.IsDismissed && m.Status != ListingStatus.Removed)
            .ToList();

        _logger.LogInformation("Checking status for {Count} matches...", matches.Count);

        foreach (var match in matches)
        {
            if (cancellationToken.IsCancellationRequested) break;

            var status = await _listingRepository.CheckStatus(match.ListingId);
            
            if (status != match.Status)
            {
                _matchResultRepository.UpdateStatus(match.ListingId, status);
                _logger.LogInformation(
                    "Match {Id} ({Title}) status changed: {Old} → {New}",
                    match.ListingId, match.ListingTitle, match.Status, status);
            }

            await Task.Delay(DelayMs, cancellationToken);
        }

        _logger.LogInformation("Status check complete.");
    }
}