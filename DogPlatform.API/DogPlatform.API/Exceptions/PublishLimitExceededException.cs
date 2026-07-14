namespace DogPlatform.API.Exceptions;

public sealed class PublishLimitExceededException() : 
    DomainException("You have reached your free publication limit", DomainErrorCode.Forbidden);