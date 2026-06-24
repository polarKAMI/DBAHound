using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Core;

namespace Web.Pages;

public class SettingsModel : PageModel
{
    private readonly IUserSettingsRepository _settingsRepository;
    private readonly ISeenListingsRepository _seenListingsRepository;
    private readonly IMatchResultRepository _matchResultRepository;

    [BindProperty]
    public UserSettings Settings { get; set; } = new();

    public string? SuccessMessage { get; set; }

    public SettingsModel(
        IUserSettingsRepository settingsRepository,
        ISeenListingsRepository seenListingsRepository,
        IMatchResultRepository matchResultRepository)
    {
        _settingsRepository = settingsRepository;
        _seenListingsRepository = seenListingsRepository;
        _matchResultRepository = matchResultRepository;
    }

    public void OnGet()
    {
        Settings = _settingsRepository.Get();
    }

    public IActionResult OnPostSave()
    {
        _settingsRepository.Save(Settings);
        SuccessMessage = "Settings saved.";
        return Page();
    }

    public IActionResult OnPostClearSeen()
    {
        _seenListingsRepository.CleanUpAll(0);
        SuccessMessage = "Seen listings cleared.";
        return Page();
    }

    public IActionResult OnPostClearDismissed()
    {
        var all = _matchResultRepository.GetAll().ToList();
        foreach (var match in all.Where(m => m.IsDismissed))
        {
            match.IsDismissed = false;
        }
        _matchResultRepository.Clear();
        _matchResultRepository.AddRange(all.Where(m => !m.IsDismissed));
        SuccessMessage = "Dismissed matches cleared.";
        return Page();
    }
}