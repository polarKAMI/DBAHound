using Microsoft.AspNetCore.Mvc.RazorPages;
using Core;
using Microsoft.AspNetCore.Mvc;

namespace Web.Pages;

public class MatchesModel : PageModel
{
    private readonly IMatchResultRepository _matchResultRepository;

    public List<StoredMatch> Matches { get; set; } = new();
    public int TotalCount { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Tab { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Sort { get; set; }

    public MatchesModel(IMatchResultRepository matchResultRepository)
    {
        _matchResultRepository = matchResultRepository;
    }

    public void OnGet()
    {
        var all = _matchResultRepository.GetAll().ToList();
        TotalCount = all.Count(m => !m.IsDismissed);

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