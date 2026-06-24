using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Core;

namespace Web.Pages.Actions;

public class WishlistActionsModel : PageModel
{
    private readonly IWishlistRepository _wishlistRepository;
    private readonly ISeenListingsRepository _seenListingsRepository;
    private readonly IBackgroundTaskQueue _taskQueue;

    public WishlistActionsModel(
        IWishlistRepository wishlistRepository,
        ISeenListingsRepository seenListingsRepository,
        IBackgroundTaskQueue taskQueue)
    {
        _wishlistRepository = wishlistRepository;
        _seenListingsRepository = seenListingsRepository;
        _taskQueue = taskQueue;
    }

    public IActionResult OnPostRemove(string title)
    {
        _wishlistRepository.Remove(title);
        return new JsonResult(new { success = true });
    }

    public IActionResult OnPostAdd([FromForm] string title, [FromForm] Platform platform)
    {
        _wishlistRepository.Add(new WishlistItem { Title = title, Platform = platform });
        _seenListingsRepository.ClearPlatform(platform);
        _taskQueue.QueuePlatformScrape(platform);
        return new JsonResult(new { success = true });
    }
}