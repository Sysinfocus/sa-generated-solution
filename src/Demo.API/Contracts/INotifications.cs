namespace Demo.API.Contracts;

public interface INotifications
{
    Task Notify<T>(T? model = default, string? message = null, NotificationAction notificationAction = NotificationAction.Read);
}