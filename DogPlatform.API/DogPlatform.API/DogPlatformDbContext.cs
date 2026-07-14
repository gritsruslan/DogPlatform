using DogPlatform.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace DogPlatform.API;

public sealed class DogPlatformDbContext(
    DbContextOptions<DogPlatformDbContext> options) : DbContext(options)
{
    public DbSet<Litter> Litters => Set<Litter>();
    
    public DbSet<BreederBenefit> BreederBenefits => Set<BreederBenefit>();
    
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(typeof(DogPlatformDbContext).Assembly);
        base.OnModelCreating(builder);
    }
}