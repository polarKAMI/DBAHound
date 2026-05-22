using Core;

namespace Web.Notifications;

public class PushoverNotificationService : INotificationService
{
    private readonly HttpClient _httpClient;
    private readonly string _token;
    private readonly string _userKey;

    public PushoverNotificationService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _token = configuration["Notifications:Pushover:Token"] ?? "";
        _userKey = configuration["Notifications:Pushover:UserKey"] ?? "";
    }

    public async Task SendAsync(string title, string message)
    {
        if (string.IsNullOrEmpty(_token) || string.IsNullOrEmpty(_userKey))
            return;

        var payload = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("token", _token),
            new KeyValuePair<string, string>("user", _userKey),
            new KeyValuePair<string, string>("title", title),
            new KeyValuePair<string, string>("message", message)
        });

        await _httpClient.PostAsync("https://api.pushover.net/1/messages.json", payload);
    }
}