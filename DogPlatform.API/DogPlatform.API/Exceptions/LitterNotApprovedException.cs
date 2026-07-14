namespace DogPlatform.API.Exceptions;

public sealed class LitterNotApprovedException() : 
    DomainException($"Only approved litters can be published");