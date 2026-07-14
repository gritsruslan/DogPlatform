namespace DogPlatform.API.Entities;

public sealed class AuditLog
{
    public int Id { get; set; }
    
    public int EntityId { get; set; }
    
    public required string Action { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
}