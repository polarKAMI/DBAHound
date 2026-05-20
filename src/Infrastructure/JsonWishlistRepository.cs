using System.Text.Json;
using Core;

namespace Infrastructure;

public class JsonWishlistRepository : IWishlistRepository
{
    private readonly string _filePath;

    public JsonWishlistRepository(string filePath)
    {
        _filePath = filePath;
    }
    
    public IEnumerable<WishlistItem> GetAll()
    {
        if (!File.Exists(_filePath))
            return new List<WishlistItem>();

        var json = File.ReadAllText(_filePath);
        return JsonSerializer.Deserialize<List<WishlistItem>>(json) ?? new List<WishlistItem>();
    }

    public void Add(WishlistItem item)
    {
        var wishlist = GetAll().ToList();
        
        wishlist.Add(item);
        
        var json = JsonSerializer.Serialize(wishlist);
        File.WriteAllText(_filePath, json);
    }
    
    public void Remove(string title)
    {
        var  wishlist = GetAll().ToList();

        wishlist.RemoveAll(w => w.Title.Equals(title, StringComparison.OrdinalIgnoreCase));
        
        var json = JsonSerializer.Serialize(wishlist);
        File.WriteAllText(_filePath, json);
    }
}