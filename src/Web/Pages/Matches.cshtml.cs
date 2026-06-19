using Microsoft.AspNetCore.Mvc.RazorPages;
using Core;
using Microsoft.AspNetCore.Mvc;

namespace Web.Pages;

public class MatchesModel : PageModel
{
    private readonly IMatchResultRepository _matchResultRepository;
    private readonly IUserSettingsRepository _settingsRepository;
    public bool AutoCleanupEnabled { get; set; }
    public int AutoCleanupDays { get; set; }

    public List<StoredMatch> Matches { get; set; } = new();
    public int TotalCount { get; set; }
    public Dictionary<string, int> PlatformCounts { get; set; } = new();
    public int FavouriteCount { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Tab { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Sort { get; set; }
    

    public MatchesModel(IMatchResultRepository matchResultRepository, IUserSettingsRepository settingsRepository)
    {
        _matchResultRepository = matchResultRepository;
        _settingsRepository = settingsRepository;
    }

    public void OnGet()
    {
        var settings = _settingsRepository.Get();
        AutoCleanupEnabled = settings.AutoCleanupEnabled;
        AutoCleanupDays = settings.AutoCleanupDays;
        
        var all = _matchResultRepository.GetAll().ToList();
        TotalCount = all.Count(m => !m.IsDismissed);
        FavouriteCount = all.Count(m => m.IsFavourite && !m.IsDismissed);
        PlatformCounts = all
            .Where(m => !m.IsDismissed && m.Platform != null)
            .GroupBy(m => m.Platform!)
            .ToDictionary(g => g.Key, g => g.Count());

        var query = all.Where(m => !m.IsDismissed).AsEnumerable();

        // Tab filter
        query = Tab switch
        {
            "Favourites" => query.Where(m => m.IsFavourite),
            "Dismissed" => all.Where(m => m.IsDismissed),
            _ when Tab != null => query.Where(m => m.Platform == Tab),
            _ => query
        };

        // Search
        if (!string.IsNullOrWhiteSpace(Search))
            query = query.Where(m =>
                m.ListingTitle.Contains(Search, StringComparison.OrdinalIgnoreCase) ||
                m.WishlistTitle.Contains(Search, StringComparison.OrdinalIgnoreCase));

        // Sort
        query = Sort switch
        {
            "PriceLow" => query.OrderBy(m => m.Price),
            "PriceHigh" => query.OrderByDescending(m => m.Price),
            "Oldest" => query.OrderBy(m => m.ListingDate),
            _ => query.OrderByDescending(m => m.ListingDate)
        };

        Matches = query.ToList();
    }

    public IActionResult OnPostDismiss(int listingId)
    {
        _matchResultRepository.Dismiss(listingId);
        return RedirectToPage(new { Tab, Search, Sort });
    }

    public IActionResult OnPostFavourite(int listingId)
    {
        _matchResultRepository.ToggleFavourite(listingId);
        return RedirectToPage(new { Tab, Search, Sort });
    }
}