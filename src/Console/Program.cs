using Core;
using Infrastructure;
using Scraper;

var httpClient = new HttpClient();
httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0");

var seenListings = new JsonSeenListingsRepository("/Users/emilpedersenschmidt/Documents/Projects/DBAHunter/data/seen.json");
var repository = new DbaListingRepository(httpClient, seenListings);
var matchingService = new MatchingService();

var wishlist = new List<WishlistItem>
{
    new WishlistItem { Title = "Jak and Daxter", Platform = Platform.PS2 },
    new WishlistItem { Title = "Tekken", Platform = Platform.PS2 },
    new WishlistItem { Title = "Burnout", Platform = Platform.PS2 }
};

Console.WriteLine("Fetching PS2 listings...");
var listings = await repository.GetByPlatform(Platform.PS2);

Console.WriteLine("Matching against wishlist...");
var matches = matchingService.Match(listings, wishlist);

if (!matches.Any())
{
    Console.WriteLine("No matches found.");
}
else
{
    foreach (var match in matches)
    {
        Console.WriteLine($"MATCH: '{match.MatchedItem.Title}' found in listing '{match.Listing.Title}' | {match.Listing.Price} kr | {match.Listing.PostalName}");
    }
}

Console.WriteLine("Done.");