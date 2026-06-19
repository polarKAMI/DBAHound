using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Core;

namespace Web.Pages.Actions;

public class SettingsActionsModel : PageModel
{
    private readonly IUserSettingsRepository _settingsRepository;

    public SettingsActionsModel(IUserSettingsRepository settingsRepository)
    {
        _settingsRepository = settingsRepository;
    }

    public IActionResult OnPostSave([FromBody] UserSettings settings)
    {
        _settingsRepository.Save(settings);
        return new JsonResult(new { success = true });
    }
}