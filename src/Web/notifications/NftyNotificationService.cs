using Core;

namespace Web.Notifications;

public class NtfyNotificationService : INotificationService
{
    private readonly HttpClient _httpClient;
    private readonly IUserSettingsRepository _settingsRepository;

    public NtfyNotificationService(HttpClient httpClient, IUserSettingsRepository settingsRepository)
    {
        _httpClient = httpClient;
        _settingsRepository = settingsRepository;
    }

    public async Task SendAsync(string title, string message)
    {
        var url = _settingsRepository.Get().NtfyUrl;
    
        if (string.IsNullOrEmpty(url))
            return;

        try
        {
            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(message)
            };
            request.Headers.Add("Title", title);
            await _httpClient.SendAsync(request);
        }
        catch
        {
            // fail silently
        }
    }
}