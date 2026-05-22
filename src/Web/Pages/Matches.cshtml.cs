using Microsoft.AspNetCore.Mvc.RazorPages;
using Core;
using Microsoft.AspNetCore.Mvc;

namespace Web.Pages;

public class MatchesModel : PageModel
{
    private readonly IMatchResultRepository _matchResultRepository;
    private readonly IListingRepository _listingRepository;
    private readonly IWishlistRepository _wishlistRepository;
    private readonly IMatchingService _matchingService;

    public List<StoredMatch> Matches { get; set; } = new();

    public MatchesModel(IMatchResultRepository matchResultRepository)
    {
        _matchResultRepository = matchResultRepository;
    }

    public void OnGet()
    {
        Matches = _matchResultRepository.GetAll()
            .Where(m => !m.IsDismissed)
            .OrderByDescending(m => m.FoundAt)
            .Take(20)
            .ToList();
    }
    public IActionResult OnPostDismiss(int listingId)
    {
        _matchResultRepository.Dismiss(listingId);
        return RedirectToPage();
    }

    public IActionResult OnPostFavourite(int listingId)
    {
        _matchResultRepository.ToggleFavourite(listingId);
        return RedirectToPage();
    }
}