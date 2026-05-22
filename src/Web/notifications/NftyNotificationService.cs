using Core;

namespace Web.Notifications;

public class NtfyNotificationService : INotificationService
{
    private readonly HttpClient _httpClient;
    private readonly string _url;

    public NtfyNotificationService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _url = configuration["Notifications:Ntfy:Url"] ?? "http://ntfy:5555/dbahound";
    }

    public async Task SendAsync(string title, string message)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, _url)
        {
            Content = new StringContent(message)
        };
        request.Headers.Add("Title", title);

        await _httpClient.SendAsync(request);
    }
}