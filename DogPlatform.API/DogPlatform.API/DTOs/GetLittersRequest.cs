namespace DogPlatform.API.DTOs;

public sealed record GetLittersRequest(string? Status, int Page, int PageSize);