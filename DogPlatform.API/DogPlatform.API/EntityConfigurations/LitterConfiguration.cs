using DogPlatform.API.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DogPlatform.API.EntityConfigurations;

internal sealed class LitterConfiguration : IEntityTypeConfiguration<Litter>
{
    public void Configure(EntityTypeBuilder<Litter> builder)
    {
        builder.HasKey(l => l.Id);

        builder
            .HasOne(l => l.Breeder)
            .WithMany(b => b.Litters)
            .HasForeignKey(l => l.BreederId);

        builder
            .Property(l => l.Status)
            .HasMaxLength(LitterStatus.MaxLength);
        
        builder
            .Property(l => l.CreatedAt)
            .ValueGeneratedOnAdd();
    }
}