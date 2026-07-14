using System.Security.Authentication;
using DogPlatform.API.Exceptions;
using DogPlatform.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace DogPlatform.API.Controllers;

[ApiController]
[Route("/api/litters")]
public sealed class LitterController(ILitterService service) : ControllerBase
{
    [Route("{litterId:int}/publish")]
    public async Task<IActionResult> PublishLitter(
        [FromRoute] int litterId,
        CancellationToken cancellationToken)
    {
        var breederId = GetBreederId();
        await service.PublishLitter(litterId, breederId, cancellationToken);
        return Ok();
    }

    private int GetBreederId()
    {
        if (!int.TryParse(Request.Headers["X-Breeder-Id"], out var breederId))
        {
            throw new UnauthorizedException();
        }
        
        return breederId;
    }
}