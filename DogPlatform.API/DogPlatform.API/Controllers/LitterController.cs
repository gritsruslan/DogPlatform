using DogPlatform.API.DTOs;
using DogPlatform.API.Exceptions;
using DogPlatform.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace DogPlatform.API.Controllers;

[ApiController]
[Route("/api/litters")]
public sealed class LitterController(ILitterService service) : ControllerBase
{
    [HttpPost("{litterId:int}/publish")]
    public async Task<IActionResult> PublishLitter(
        [FromRoute] int litterId,
        CancellationToken cancellationToken)
    {
        var breederId = GetBreederId(Request);
        await service.PublishLitter(litterId, breederId, cancellationToken);
        return Ok();
    }

    [HttpGet]
    public async Task<IActionResult> GetLitters(
        [FromQuery] GetLittersRequest request,
        CancellationToken cancellationToken)
    {
        var breederId = GetBreederId(Request);
        var data = await service.GetLitters(request, breederId, cancellationToken);
        return Ok(data);
    }

    private static int GetBreederId(HttpRequest request)
    {
        if (!int.TryParse(request.Headers["X-Breeder-Id"], out var breederId))
        {
            throw new UnauthorizedException();
        }
        
        return breederId;
    }
}