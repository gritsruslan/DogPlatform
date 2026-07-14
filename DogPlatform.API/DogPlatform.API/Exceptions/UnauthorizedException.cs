namespace DogPlatform.API.Exceptions;

public sealed class UnauthorizedException() : DomainException(string.Empty, DomainErrorCode.Unauthorized);