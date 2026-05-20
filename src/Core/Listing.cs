namespace Core;

public class Listing
{
    public required int Id { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public string? Condition { get; set; }
    public decimal Price { get; set; }
    public DateTime? LastEdited { get; set; }
    public Platform Platform { get; set; }
    public string? PostalCode { get; set; }
    public string? PostalName { get; set; }
    public bool? IsLot { get; set; }
}