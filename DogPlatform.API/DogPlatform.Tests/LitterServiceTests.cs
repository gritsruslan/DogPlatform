using DogPlatform.API;
using DogPlatform.API.Entities;
using DogPlatform.API.Exceptions;
using DogPlatform.API.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace DogPlatform.Tests;

public sealed class LitterServiceTests
{
    private readonly Mock<INotificationService> _notificationService = new();

    private static async Task<(DogPlatformDbContext Db, SqliteConnection Connection)> CreateDbContext()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<DogPlatformDbContext>()
            .UseSqlite(connection)
            .Options;

        var dbContext = new DogPlatformDbContext(options);

        await dbContext.Database.EnsureCreatedAsync();

        return (dbContext, connection);
    }

    [Fact]
    public async Task PublishLitter_ShouldPublishLitter_WhenDataIsValid()
    {
        var (db, connection) = await CreateDbContext();

        await using (db)
        await using (connection)
        {
            db.Litters.Add(new Litter
            {
                Id = 1,
                BreederId = 10,
                Status = LitterStatus.Approved,
                CreatedAt = DateTimeOffset.UtcNow,
                Breeder = null!
            });

            db.BreederBenefits.Add(new BreederBenefit
            {
                BreederId = 10,
                FreeLimit = 3,
                UsedCount = 0,
                Litters = []
            });

            await db.SaveChangesAsync();

            var service = new LitterService(db, _notificationService.Object);

            await service.PublishLitter(1, 10, CancellationToken.None);

            var litter = await db.Litters.FindAsync(1);

            Assert.NotNull(litter);
            Assert.Equal(LitterStatus.Published, litter.Status);

            var benefit = await db.BreederBenefits.FirstAsync();

            Assert.Equal(1, benefit.UsedCount);

            Assert.Single(db.AuditLogs);

            _notificationService.Verify(x =>
                    x.SendEmail(
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<string>()),
                Times.Once);
        }
    }

    [Fact]
    public async Task PublishLitter_ShouldThrow_WhenLitterDoesNotExist()
    {
        var (db, connection) = await CreateDbContext();

        await using (db)
        await using (connection)
        {
            var service = new LitterService(db, _notificationService.Object);

            await Assert.ThrowsAsync<LitterNotFoundException>(() =>
                service.PublishLitter(1, 10, CancellationToken.None));
        }
    }

    [Fact]
    public async Task PublishLitter_ShouldThrow_WhenBreederIsNotOwner()
    {
        var (db, connection) = await CreateDbContext();

        await using (db)
        await using (connection)
        {
            db.BreederBenefits.Add(new BreederBenefit
            {
                BreederId = 5,
                FreeLimit = 3,
                UsedCount = 0,
                Litters = []
            });

            db.Litters.Add(new Litter
            {
                Id = 1,
                BreederId = 5,
                Status = LitterStatus.Approved,
                CreatedAt = DateTimeOffset.UtcNow,
                Breeder = null!
            });

            await db.SaveChangesAsync();

            var service = new LitterService(db, _notificationService.Object);

            await Assert.ThrowsAsync<ForbiddenException>(() =>
                service.PublishLitter(1, 10, CancellationToken.None));
        }
    }

    [Fact]
    public async Task PublishLitter_ShouldThrow_WhenLitterIsNotApproved()
    {
        var (db, connection) = await CreateDbContext();

        await using (db)
        await using (connection)
        {
            db.Litters.Add(new Litter
            {
                Id = 1,
                BreederId = 10,
                Status = LitterStatus.Draft,
                CreatedAt = DateTimeOffset.UtcNow,
                Breeder = null!
            });

            db.BreederBenefits.Add(new BreederBenefit
            {
                BreederId = 10,
                FreeLimit = 3,
                UsedCount = 0,
                Litters = []
            });

            await db.SaveChangesAsync();

            var service = new LitterService(db, _notificationService.Object);

            await Assert.ThrowsAsync<LitterNotApprovedException>(() =>
                service.PublishLitter(1, 10, CancellationToken.None));
        }
    }

    [Fact]
    public async Task PublishLitter_ShouldThrow_WhenPublishLimitExceeded()
    {
        var (db, connection) = await CreateDbContext();

        await using (db)
        await using (connection)
        {
            db.Litters.Add(new Litter
            {
                Id = 1,
                BreederId = 10,
                Status = LitterStatus.Approved,
                CreatedAt = DateTimeOffset.UtcNow,
                Breeder = null!
            });

            db.BreederBenefits.Add(new BreederBenefit
            {
                BreederId = 10,
                FreeLimit = 3,
                UsedCount = 3,
                Litters = []
            });

            await db.SaveChangesAsync();

            var service = new LitterService(db, _notificationService.Object);

            await Assert.ThrowsAsync<PublishLimitExceededException>(() =>
                service.PublishLitter(1, 10, CancellationToken.None));

            var litter = await db.Litters.FindAsync(1);

            Assert.NotNull(litter);
            Assert.Equal(LitterStatus.Approved, litter.Status);

            var benefit = await db.BreederBenefits.FirstAsync();

            Assert.Equal(3, benefit.UsedCount);

            Assert.Empty(db.AuditLogs);

            _notificationService.Verify(x =>
                    x.SendEmail(
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<string>()),
                Times.Never);
        }
    }
}