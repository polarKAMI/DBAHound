namespace Core;

public interface IWishlistRepository
{
    IEnumerable<WishlistItem> GetAll();
    void Add(WishlistItem item);
    void Remove(string title);
}