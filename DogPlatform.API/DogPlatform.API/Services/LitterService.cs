using DogPlatform.API.DTOs;
using DogPlatform.API.Entities;
using DogPlatform.API.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace DogPlatform.API.Services;

public sealed class LitterService(
    DogPlatformDbContext dbContext, 
    INotificationService notificationService) : ILitterService
{
    public async Task PublishLitter(
        int litterId, 
        int breederId, 
        CancellationToken cancellationToken)
    {
        var litter = await dbContext.Litters
            .FirstOrDefaultAsync(l => l.Id == litterId, cancellationToken);
        
        if (litter is null)
        {
            throw new LitterNotFoundException(litterId);
        }

        if (litter.BreederId != breederId)
        {
            throw new ForbiddenException();
        }

        if (litter.Status != LitterStatus.Approved)
        {
            throw new LitterNotApprovedException(litterId);
        }
        
        var breederBenefit = await dbContext.BreederBenefits
            .Where(b => b.BreederId == breederId)
            .FirstOrDefaultAsync(cancellationToken);

        if (breederBenefit is null)
        {
            throw new UnauthorizedException();
        }
        
        if (breederBenefit.UsedCount >= breederBenefit.FreeLimit)
        {
            await dbContext.AuditLogs.AddAsync(new AuditLog
            {
                EntityId = litterId,
                Action = "Publish attempt failed - limits exceeded",
                CreatedAt = DateTimeOffset.UtcNow
            }, cancellationToken);
            
            await dbContext.SaveChangesAsync(cancellationToken);
            throw new PublishLimitExceededException();
        }
        
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            breederBenefit.UsedCount++;
            litter.Status = LitterStatus.Published;
            await dbContext.AddAsync(new AuditLog
            {
                EntityId = litterId,
                Action = "Published for free",
                CreatedAt = DateTimeOffset.UtcNow
            }, cancellationToken);
            
            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
        
        // Would be better to dispatch message in queue and process it later
        await notificationService.SendEmail(
            "breeder@example.com", 
            "Litter published", 
            $"Litter with id {litterId}  has been published");
    }

    public async Task<PagedData<Litter>> GetLitters(
        GetLittersRequest request, 
        int breederId, 
        CancellationToken cancellationToken)
    {
        var (status, page, pageSize) = request;

        var breederExists = await dbContext.BreederBenefits
            .AnyAsync(b => b.BreederId == breederId, cancellationToken);

        if (!breederExists)
        {
            throw new UnauthorizedException();
        }
        
        int skip = (page - 1) * pageSize;
        int take = pageSize;

        var query = dbContext.Litters
            .Where(l => l.BreederId == breederId)
            .AsQueryable();

        if (status is not null)
        {
            query = query.Where(l => l.Status == status);
        }

        var items = await query
            .Skip(skip).Take(take)
            .ToListAsync(cancellationToken);
        
        var totalCount = await query.CountAsync(cancellationToken);
        
        return new PagedData<Litter>(items, totalCount, page, pageSize);
    }
}