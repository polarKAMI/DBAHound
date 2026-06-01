namespace Core;

public class UserSettings
{
    public int ScrapeIntervalHours { get; set; } = 24;
    public string NotificationProvider { get; set; } = "None";
    public string PushoverToken { get; set; } = string.Empty;
    public string PushoverUserKey { get; set; } = string.Empty;
    public string NtfyUrl { get; set; } = string.Empty;
}