namespace DogPlatform.API.Exceptions;

public sealed class LitterNotApprovedException(int litterId) : 
    DomainException($"Litter with {litterId} was not found");