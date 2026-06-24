    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using Core;

    namespace Web.Pages;
    public class MatchActionsModel : PageModel
    {
        private readonly IMatchResultRepository _matchResultRepository;
        private readonly IListingRepository _listingRepository;

        public MatchActionsModel(IMatchResultRepository matchResultRepository,  IListingRepository listingRepository)
        {
            _matchResultRepository = matchResultRepository;
            _listingRepository =  listingRepository;
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
        
        public async Task<IActionResult> OnPostCheckStatus()
        {
            var matches = _matchResultRepository.GetAll()
                .Where(m => !m.IsDismissed && m.Status != ListingStatus.Removed)
                .ToList();

            foreach (var match in matches)
            {
                var status = await _listingRepository.CheckStatus(match.ListingId);
                if (status != match.Status)
                    _matchResultRepository.UpdateStatus(match.ListingId, status);
            }

            var updated = _matchResultRepository.GetAll()
                .Select(m => new { m.ListingId, status = m.Status.ToString() })
                .ToList();

            return new JsonResult(new { success = true, matches = updated });
        }
    }