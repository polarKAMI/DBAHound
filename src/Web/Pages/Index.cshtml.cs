using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Core;

namespace Web.Pages;

public class IndexModel : PageModel
{
    private readonly IMatchResultRepository _matchResultRepository;
    private readonly IListingRepository _listingRepository;
    private readonly IWishlistRepository _wishlistRepository;
    private readonly IMatchingService _matchingService;

    public List<StoredMatch> Matches { get; set; } = new();

    public IndexModel(
        IMatchResultRepository matchResultRepository,
        IListingRepository listingRepository,
        IWishlistRepository wishlistRepository,
        IMatchingService matchingService)
    {
        _matchResultRepository = matchResultRepository;
        _listingRepository = listingRepository;
        _wishlistRepository = wishlistRepository;
        _matchingService = matchingService;
    }

    public void OnGet()
    {
        Matches = _matchResultRepository.GetAll().ToList();
    }

    public async Task<IActionResult> OnPostScrape()
    {
        var wishlist = _wishlistRepository.GetAll().ToList();
        var allListings = new List<Listing>();

        foreach (var platform in wishlist.Select(w => w.Platform).Distinct())
        {
            var listings = await _listingRepository.GetByPlatform(platform);
            allListings.AddRange(listings);
        }

        var matches = _matchingService.Match(allListings, wishlist);

        var storedMatches = matches.Select(m => new StoredMatch
        {
            ListingId = m.Listing.Id,
            ListingTitle = m.Listing.Title,
            WishlistTitle = m.MatchedItem.Title,
            Price = m.Listing.Price,
            PostalName = m.Listing.PostalName,
            FoundAt = DateTime.Now
        }).ToList();

        _matchResultRepository.Clear();
        _matchResultRepository.AddRange(storedMatches);

        return RedirectToPage();
    }
}