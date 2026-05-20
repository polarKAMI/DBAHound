namespace Core;

public class MatchingService : IMatchingService
{
    public IEnumerable<MatchResult> Match(IEnumerable<Listing> listings, IEnumerable<WishlistItem> wishlistItems)
    {
        List<MatchResult> matchResults = new List<MatchResult>();
        
        foreach (var listing in listings)
        {
            foreach (var wishlistItem in wishlistItems)
            {
                bool titleMatch = listing.Title.Contains(wishlistItem.Title, StringComparison.OrdinalIgnoreCase);
                bool descriptionMatch = listing.Description != null && 
                                        listing.Description.Contains(wishlistItem.Title, StringComparison.OrdinalIgnoreCase);

                if (titleMatch || descriptionMatch)
                {
                    matchResults.Add(new MatchResult
                    {
                        Listing = listing,
                        MatchedItem = wishlistItem
                    });
                }
            }
        }
        return matchResults;
    } 
}