namespace DogPlatform.API.Services;

public sealed class ConsoleNotificationService : INotificationService
{
    public Task SendEmail(string recipient, string subject, string body)
    {
        Console.WriteLine($"Send email to {recipient}.\nSubject: {subject}.\nBody: {body}");
        return Task.CompletedTask;
    }
}