namespace DogPlatform.API.Exceptions;

public sealed class LitterNotApprovedException() : 
    DomainException("You have reached your free publication limit");