namespace Core;

public class UserSettings
{
    //Scraper settings
    public string ScheduleMode { get; set; } = "Hourly";
    public int ScrapeIntervalHours { get; set; } = 24;
    public int ScrapeTimeHour { get; set; } = 8;
    public int ScrapeTimeMinute { get; set; } = 0;
    public DayOfWeek ScrapeDay { get; set; } = DayOfWeek.Monday;
    
    //Cleanup settings
    public bool AutoCleanupEnabled { get; set; } = false;
    public int AutoCleanupDays { get; set; } = 30;
    
    //Notification settings
    public string NotificationProvider { get; set; } = "None";
    public string PushoverToken { get; set; } = string.Empty;
    public string PushoverUserKey { get; set; } = string.Empty;
    public string NtfyUrl { get; set; } = string.Empty;
}