using DogPlatform.API.DTOs;
using DogPlatform.API.Entities;

namespace DogPlatform.API.Services;

public interface ILitterService
{
    public Task PublishLitter(int litterId, int breederId, CancellationToken cancellationToken);
    
    public Task<PagedData<Litter>> GetLitters(
        GetLittersRequest request, 
        int breederId, 
        CancellationToken cancellationToken);
}