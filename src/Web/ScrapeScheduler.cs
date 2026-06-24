using Core;
using Infrastructure;

namespace Web;

public class ScrapeScheduler : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IUserSettingsRepository _settingsRepository;
    private readonly ILogger<ScrapeScheduler> _logger;
    private readonly IBackgroundTaskQueue _taskQueue;

    public ScrapeScheduler(
        IServiceProvider serviceProvider,
        ILogger<ScrapeScheduler> logger,
        IUserSettingsRepository settingsRepository,
        IBackgroundTaskQueue taskQueue)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _settingsRepository = settingsRepository;
        _taskQueue = taskQueue;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Start queue listener alongside scheduled scrapes
        var queueTask = ProcessQueueAsync(stoppingToken);
    
        await RunScrape(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            var settings = _settingsRepository.Get();
            var delay = GetDelayUntilNextRun(settings);
            _logger.LogInformation("Next scrape in {delay}", delay);
            await Task.Delay(delay, stoppingToken);
            await RunScrape(stoppingToken);
        }

        await queueTask;
    }

    private async Task ProcessQueueAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var platform = await _taskQueue.DequeueAsync(stoppingToken);
            if (platform is null) continue;
        
            _logger.LogInformation("Queue triggered scrape for {platform}", platform.Value);
            await RunPlatformScrape(platform.Value, stoppingToken);
        }
    }
    
    private TimeSpan GetDelayUntilNextRun(UserSettings settings)
    {
        var now = DateTime.Now;

        return settings.ScheduleMode switch
        {
            "Daily" => GetDelayUntilTime(now, settings.ScrapeTimeHour, settings.ScrapeTimeMinute),
            "Weekly" => GetDelayUntilDayAndTime(now, settings.ScrapeDay, settings.ScrapeTimeHour, settings.ScrapeTimeMinute),
            _ => TimeSpan.FromHours(settings.ScrapeIntervalHours)
        };
    }

    private TimeSpan GetDelayUntilTime(DateTime now, int hour, int minute)
    {
        var next = now.Date.AddHours(hour).AddMinutes(minute);
        if (next <= now)
            next = next.AddDays(1);
        return next - now;
    }

    private TimeSpan GetDelayUntilDayAndTime(DateTime now, DayOfWeek day, int hour, int minute)
    {
        var next = now.Date.AddHours(hour).AddMinutes(minute);
        int daysUntil = ((int)day - (int)now.DayOfWeek + 7) % 7;
        if (daysUntil == 0 && next <= now)
            daysUntil = 7;
        next = next.AddDays(daysUntil);
        return next - now;
    }
    private async Task RunScrape(CancellationToken stoppingToken)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var wishlistRepository = scope.ServiceProvider.GetRequiredService<IWishlistRepository>();
            var platforms = wishlistRepository.GetAll().Select(w => w.Platform).Distinct().ToList();
        
            if (!platforms.Any())
            {
                _logger.LogInformation("Wishlist is empty, skipping scrape.");
                return;
            }

            _logger.LogInformation("Starting scheduled scrape...");
            await RunScrapeForPlatforms(platforms, stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Scrape failed.");
        }
    }

    private async Task RunPlatformScrape(Platform platform, CancellationToken stoppingToken)
    {
        try
        {
            _logger.LogInformation("Queue triggered scrape for {platform}", platform);
            await RunScrapeForPlatforms(new[] { platform }, stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Platform scrape failed for {platform}.", platform);
        }
    }

    private async Task RunScrapeForPlatforms(IEnumerable<Platform> platforms, CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var wishlistRepository = scope.ServiceProvider.GetRequiredService<IWishlistRepository>();
        var listingRepository = scope.ServiceProvider.GetRequiredService<IListingRepository>();
        var matchingService = scope.ServiceProvider.GetRequiredService<IMatchingService>();
        var matchResultRepository = scope.ServiceProvider.GetRequiredService<IMatchResultRepository>();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
        var seenListingsRepository = scope.ServiceProvider.GetRequiredService<ISeenListingsRepository>();

        var wishlist = wishlistRepository.GetAll()
            .Where(w => platforms.Contains(w.Platform))
            .ToList();

        if (!wishlist.Any())
        {
            _logger.LogInformation("No wishlist items for specified platforms, skipping.");
            return;
        }

        var allListings = new List<Listing>();
        foreach (var platform in platforms)
        {
            if (stoppingToken.IsCancellationRequested) break;
            var listings = await listingRepository.GetByPlatform(platform);
            allListings.AddRange(listings);
        }

        var matches = matchingService.Match(allListings, wishlist);
        var existing = matchResultRepository.GetAll().Select(m => m.ListingId).ToHashSet();

        var newMatches = matches
            .Where(m => !existing.Contains(m.Listing.Id))
            .Select(m => new StoredMatch
            {
                ListingId = m.Listing.Id,
                ListingTitle = m.Listing.Title,
                WishlistTitle = m.MatchedItem.Title,
                Price = m.Listing.Price,
                PostalName = m.Listing.PostalName,
                FoundAt = DateTime.Now,
                ListingDate = m.Listing.LastEdited,
                ImageUrl = m.Listing.ImageUrl,
                Platform = m.Listing.Platform.ToString()
            }).ToList();

        matchResultRepository.AddRange(newMatches);

        foreach (var platform in platforms)
            seenListingsRepository.CleanUp(platform);

        var settings = _settingsRepository.Get();
        if (settings.AutoCleanupEnabled)
            matchResultRepository.Cleanup(settings.AutoCleanupDays);

        if (newMatches.Count > 0)
        {
            var platformList = string.Join(", ", platforms);
            var summary = string.Join("\n", newMatches.Select(m => $"{m.WishlistTitle} — {m.Price} kr — {m.PostalName}"));
            await notificationService.SendAsync($"DBAHound — {newMatches.Count} new matches", summary);
        }

        _logger.LogInformation("Scrape complete. {Count} new matches found.", newMatches.Count);
    }
}