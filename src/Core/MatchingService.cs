namespace Core;

public class MatchingService : IMatchingService
{
    public IEnumerable<MatchResult> Match(IEnumerable<Listing> listings, IEnumerable<WishlistItem> wishlistItems)
    {
        var matchResults = new List<MatchResult>();

        foreach (var listing in listings)
        {
            foreach (var wishlistItem in wishlistItems)
            {
                if (IsMatch(listing.Title, wishlistItem.Title) ||
                    (listing.Description != null && IsMatch(listing.Description, wishlistItem.Title)))
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

    private bool IsMatch(string text, string wishlistTitle)
    {
        text = text.ToLowerInvariant();
        wishlistTitle = wishlistTitle.ToLowerInvariant();

        var wishlistWords = wishlistTitle
            .Split(' ', StringSplitOptions.RemoveEmptyEntries);

        var textWords = text
            .Split(new char[] { ' ', ',', '.', '-', '(', ')', ':', '/' },
                StringSplitOptions.RemoveEmptyEntries)
            .ToHashSet();

        if (!wishlistWords.All(w => textWords.Contains(w)))
            return false;

        var lastWishlistWord = wishlistWords.Last();
        if (!int.TryParse(lastWishlistWord, out _))
        {
            var textWordList = text
                .Split(new char[] { ' ', ',', '.', '-', '(', ')', ':', '/' },
                    StringSplitOptions.RemoveEmptyEntries)
                .ToList();

            for (int i = 0; i < textWordList.Count - 1; i++)
            {
                if (textWordList[i] == lastWishlistWord &&
                    int.TryParse(textWordList[i + 1], out int followingNumber) &&
                    followingNumber >= 2 && followingNumber <= 10)
                {
                    return false;
                }
            }
        }

        return true;
    }
}