    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using Core;

    namespace Web.Pages;
    public class MatchActionsModel : PageModel
    {
        private readonly IMatchResultRepository _matchResultRepository;

        public MatchActionsModel(IMatchResultRepository matchResultRepository)
        {
            _matchResultRepository = matchResultRepository;
        }

        public IActionResult OnPostFavourite(int listingId)
        {
            _matchResultRepository.ToggleFavourite(listingId);
            var match = _matchResultRepository.GetAll()
                .FirstOrDefault(m => m.ListingId == listingId);
            return new JsonResult(new { isFavourite = match?.IsFavourite ?? false });
        }

        public IActionResult OnPostDismiss(int listingId)
        {
            _matchResultRepository.Dismiss(listingId);
            return new JsonResult(new { success = true });
        }
        
        public IActionResult OnPostRestore(int listingId)
        {
            _matchResultRepository.Restore(listingId);
            return new JsonResult(new { success = true });
        }
    }