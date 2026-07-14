namespace DogPlatform.API.Exceptions;

public sealed class PublishLimitExceededException() : 
    DomainException("Publish limit exceeded", DomainErrorCode.Forbidden);