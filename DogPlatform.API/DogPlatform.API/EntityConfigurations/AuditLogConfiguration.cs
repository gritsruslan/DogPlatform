using DogPlatform.API.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DogPlatform.API.EntityConfigurations;

internal sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder
            .HasKey(a => a.Id);

        builder
            .Property(a => a.Action)
            .HasMaxLength(AuditLog.ActionMaxLength);
        
        builder
            .Property(a => a.CreatedAt)
            .ValueGeneratedOnAdd();
    }
}