using Core;

namespace Web.Notifications;
public class DynamicNotificationService : INotificationService
{
    private readonly PushoverNotificationService _pushover;
    private readonly NtfyNotificationService _ntfy;
    private readonly IUserSettingsRepository _settingsRepository;

    public DynamicNotificationService(
        PushoverNotificationService pushover,
        NtfyNotificationService ntfy,
        IUserSettingsRepository settingsRepository)
    {
        _pushover = pushover;
        _ntfy = ntfy;
        _settingsRepository = settingsRepository;
    }

    public async Task SendAsync(string title, string message)
    {
        var provider = _settingsRepository.Get().NotificationProvider;
        INotificationService service = provider switch
        {
            "Pushover" => _pushover,
            "Ntfy" => _ntfy,
            _ => null
        };

        if (service != null)
            await service.SendAsync(title, message);
    }
}