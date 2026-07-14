namespace DogPlatform.API.Exceptions;

public sealed class ForbiddenException() : DomainException(string.Empty, DomainErrorCode.Forbidden);