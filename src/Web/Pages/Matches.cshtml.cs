using Microsoft.AspNetCore.Mvc.RazorPages;
using Core;

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
            .OrderByDescending(m => m.FoundAt)
            .ToList();
    }
}