using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Core;

namespace Web.Pages;

public class WishlistActionsModel : PageModel
{
    private readonly IWishlistRepository _wishlistRepository;

    public WishlistActionsModel(IWishlistRepository wishlistRepository)
    {
        _wishlistRepository = wishlistRepository;
    }

    public IActionResult OnPostRemove(string title)
    {
        _wishlistRepository.Remove(title);
        return new JsonResult(new { success = true });
    }
}