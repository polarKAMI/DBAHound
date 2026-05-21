namespace Core;

public class StoredMatch
{
    public int ListingId { get; set; }
    public string ListingTitle { get; set; } = string.Empty;
    public string WishlistTitle { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string? PostalName { get; set; }
    public DateTime FoundAt { get; set; }
}