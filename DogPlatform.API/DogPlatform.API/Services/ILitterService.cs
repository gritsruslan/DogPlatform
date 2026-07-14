namespace DogPlatform.API.Services;

public interface ILitterService
{
    public Task PublishLitter(int litterId, int breederId, CancellationToken cancellationToken);
}