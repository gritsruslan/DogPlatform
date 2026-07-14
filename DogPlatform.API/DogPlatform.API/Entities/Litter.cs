namespace DogPlatform.API.Entities;

public sealed class Litter
{
    public int Id { get; set; }
    
    public int BreederId { get; set; }
    
    public required string Status { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}