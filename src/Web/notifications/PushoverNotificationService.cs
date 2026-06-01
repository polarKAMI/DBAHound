using Core;

namespace Web.Notifications;

public class PushoverNotificationService : INotificationService
{
    private readonly HttpClient _httpClient;
    private readonly IUserSettingsRepository _settingsRepository;

    public PushoverNotificationService(HttpClient httpClient, IUserSettingsRepository settingsRepository)
    {
        _httpClient = httpClient;
        _settingsRepository = settingsRepository;
    }

    public async Task SendAsync(string title, string message)
    {
        var settings = _settingsRepository.Get();
    
        if (string.IsNullOrEmpty(settings.PushoverToken) || string.IsNullOrEmpty(settings.PushoverUserKey))
            return;

        var payload = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("token", settings.PushoverToken),
            new KeyValuePair<string, string>("user", settings.PushoverUserKey),
            new KeyValuePair<string, string>("title", title),
            new KeyValuePair<string, string>("message", message)
        });

        await _httpClient.PostAsync("https://api.pushover.net/1/messages.json", payload);
    }
}