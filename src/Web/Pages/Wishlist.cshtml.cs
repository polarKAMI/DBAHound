using Microsoft.AspNetCore.Mvc.RazorPages;
using Core;
using Microsoft.AspNetCore.Mvc;

namespace Web.Pages;

public class Wishlist : PageModel
{
    private readonly IWishlistRepository _wishlistRepository;
    
    public List<WishlistItem> WishlistItems { get; set; } = new();

    public Wishlist(IWishlistRepository wishlistRepository)
    {
        _wishlistRepository = wishlistRepository;
    }
    public void OnGet()
    {
        WishlistItems = _wishlistRepository.GetAll().ToList();
    }
    
    [BindProperty]
    public string NewTitle { get; set; } = string.Empty;

    [BindProperty]
    public Platform NewPlatform { get; set; }

    public IActionResult OnPostAdd()
    {
        if (!string.IsNullOrWhiteSpace(NewTitle))
            _wishlistRepository.Add(new WishlistItem { Title = NewTitle, Platform = NewPlatform });
    
        return RedirectToPage();
    }

    public IActionResult OnPostRemove(string title)
    {
        _wishlistRepository.Remove(title);
        return RedirectToPage();
    }
}