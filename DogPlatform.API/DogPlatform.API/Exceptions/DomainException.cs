namespace DogPlatform.API.Exceptions;

public class DomainException(
    string? message = null,
    DomainErrorCode errorCode = DomainErrorCode.BadRequest) : Exception(message)
{
    public DomainErrorCode ErrorCode => errorCode;
}