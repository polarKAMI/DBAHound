namespace Core;

public enum ListingStatus
{
    Active,
    Sold,       // disposed: true
    Removed,    // 404
    Unknown     // fetch failed / parse error
}