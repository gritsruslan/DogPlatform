namespace DogPlatform.API.Exceptions;

public sealed class UnauthorizedException() : DomainException(null, DomainErrorCode.Unauthorized);