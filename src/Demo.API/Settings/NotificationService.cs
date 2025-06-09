namespace Demo.API.Settings;

public sealed class NotificationService : INotifications
{
    public Task Notify<T>(
        T? model = default,
        string? message = null,
        NotificationAction notificationAction = NotificationAction.Read)
    {
        Console.WriteLine(notificationAction.ToString() + ": " + typeof(T).Name + ", " + message);
        return Task.CompletedTask;
    }
}