namespace DogPlatform.API.Exceptions;

public sealed class ForbiddenException() : DomainException(null, DomainErrorCode.Forbidden);