namespace Core;

public interface INotificationService
{
    Task SendAsync(string title, string message);
}