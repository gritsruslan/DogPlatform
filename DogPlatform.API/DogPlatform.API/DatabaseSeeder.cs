using DogPlatform.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace DogPlatform.API;

internal static class DatabaseSeeder
{
    public static async Task SeedAsync(DogPlatformDbContext dbContext)
    {
        if (await dbContext.Litters.AnyAsync())
        {
            return;
        }
        
        var breederId = 42;

        dbContext.BreederBenefits.Add(new BreederBenefit
        {
            BreederId = breederId,
            FreeLimit = 3,
            UsedCount = 1,
            Litters = [
                new Litter
                {
                    Id = 1,
                    BreederId = breederId,
                    Status = LitterStatus.Approved,
                    CreatedAt = DateTimeOffset.UtcNow,
                    Breeder = null!
                },
                new Litter
                {
                    Id = 2,
                    BreederId = breederId,
                    Status = LitterStatus.Draft,
                    CreatedAt = DateTimeOffset.UtcNow,
                    Breeder = null!
                },
                new Litter
                {
                    Id = 3,
                    BreederId = breederId,
                    Status = LitterStatus.Published,
                    CreatedAt = DateTimeOffset.UtcNow,
                    Breeder = null!
                }]
        });
        
        await dbContext.SaveChangesAsync();
    }
}