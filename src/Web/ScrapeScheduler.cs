using Core;
using Infrastructure;

namespace Web;

public class ScrapeScheduler : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IUserSettingsRepository _settingsRepository;
    private readonly ILogger<ScrapeScheduler> _logger;
    private readonly TimeSpan _interval;

    public ScrapeScheduler(IServiceProvider serviceProvider, ILogger<ScrapeScheduler> logger, IUserSettingsRepository settingsRepository)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _settingsRepository = settingsRepository;
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

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await RunScrape(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            var settings = _settingsRepository.Get();
            var delay = GetDelayUntilNextRun(settings);
            _logger.LogInformation("Next scrape in {delay}", delay);
            await Task.Delay(delay, stoppingToken);
            await RunScrape(stoppingToken);
        }
    }

    private async Task RunScrape(CancellationToken stoppingToken)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var wishlistRepository = scope.ServiceProvider.GetRequiredService<IWishlistRepository>();
            var listingRepository = scope.ServiceProvider.GetRequiredService<IListingRepository>();
            var matchingService = scope.ServiceProvider.GetRequiredService<IMatchingService>();
            var matchResultRepository = scope.ServiceProvider.GetRequiredService<IMatchResultRepository>();
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
            var seenListingsRepository = scope.ServiceProvider.GetRequiredService<ISeenListingsRepository>();

            var wishlist = wishlistRepository.GetAll().ToList();
            if (!wishlist.Any())
            {
                _logger.LogInformation("Wishlist is empty, skipping scrape.");
                return;
            }

            _logger.LogInformation("Starting scheduled scrape...");
            var allListings = new List<Listing>();

            foreach (var platform in wishlist.Select(w => w.Platform).Distinct())
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
            seenListingsRepository.CleanUp();
            seenListingsRepository.CleanUp();

            var currentSettings = _settingsRepository.Get();
            if (currentSettings.AutoCleanupEnabled)
                matchResultRepository.Cleanup(currentSettings.AutoCleanupDays);
            
            if (newMatches.Count > 0)
            {
                var summary = string.Join("\n", newMatches.Select(m => $"{m.WishlistTitle} — {m.Price} kr — {m.PostalName}"));
                await notificationService.SendAsync($"DBAHound — {newMatches.Count} new matches", summary);
            }
            _logger.LogInformation("Scrape complete. {Count} new matches found.", newMatches.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Scrape failed.");
        }
    }
}