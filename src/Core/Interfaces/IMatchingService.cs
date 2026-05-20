namespace Core;

public interface IMatchingService
{
    IEnumerable<MatchResult> Match(
        IEnumerable<Listing> listings,
        IEnumerable<WishlistItem> wishlist);
}