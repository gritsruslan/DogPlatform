namespace DogPlatform.API.Entities;

public sealed class BreederBenefit
{
    public int BreederId { get; set; }
    
    public int FreeLimit { get; set; }
    
    public int UsedCount { get; set; }
}