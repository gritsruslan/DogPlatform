namespace DogPlatform.API.Services;

public interface INotificationService
{
    public Task SendEmail(string recipient, string subject, string body);
}