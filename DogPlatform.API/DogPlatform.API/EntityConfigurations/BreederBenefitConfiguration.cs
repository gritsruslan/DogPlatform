using DogPlatform.API.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DogPlatform.API.EntityConfigurations;

internal sealed class BreederBenefitConfiguration : IEntityTypeConfiguration<BreederBenefit>
{
    public void Configure(EntityTypeBuilder<BreederBenefit> builder)
    {
        builder.HasKey(b => b.BreederId);
    }
}