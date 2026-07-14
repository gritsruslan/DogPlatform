namespace DogPlatform.API.Exceptions;

public sealed class LitterNotFoundException(int litterId) :
    DomainException($"Litter with id  {litterId} was not found");