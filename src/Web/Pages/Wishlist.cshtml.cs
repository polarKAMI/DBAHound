using Microsoft.AspNetCore.Mvc.RazorPages;
using Core;
using Microsoft.AspNetCore.Mvc;

namespace Web.Pages;

public class WishlistModel : PageModel
{
    private readonly IWishlistRepository _wishlistRepository;
    
    public List<WishlistItem> WishlistItems { get; set; } = new();

    public WishlistModel(IWishlistRepository wishlistRepository)
    {
        _wishlistRepository = wishlistRepository;
    }

    public void OnGet()
    {
        WishlistItems = _wishlistRepository.GetAll().ToList();
    }
}