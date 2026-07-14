using DogPlatform.API;
using DogPlatform.API.DTOs;
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
    
    [Fact]
    public async Task GetLitters_ShouldReturnFilteredPagedData()
    {
        var (db, connection) = await CreateDbContext();

        await using (db)
        await using (connection)
        {
            db.BreederBenefits.AddRange(new BreederBenefit
            {
                BreederId = 1,
                FreeLimit = 3,
                UsedCount = 0,
                Litters = []
            },
            new BreederBenefit
            {
                BreederId = 2,
                FreeLimit = 10,
                UsedCount = 10,
                Litters = []
            });

            db.Litters.AddRange(
                new Litter
                {
                    Id = 1,
                    BreederId = 1,
                    Status = LitterStatus.Approved,
                    CreatedAt = DateTimeOffset.UtcNow,
                    Breeder = null!
                },
                new Litter
                {
                    Id = 2,
                    BreederId = 1,
                    Status = LitterStatus.Approved,
                    CreatedAt = DateTimeOffset.UtcNow,
                    Breeder = null!
                },
                new Litter
                {
                    Id = 3,
                    BreederId = 1,
                    Status = LitterStatus.Draft,
                    CreatedAt = DateTimeOffset.UtcNow,
                    Breeder = null!
                },
                new Litter
                {
                    Id = 4,
                    BreederId = 2,
                    Status = LitterStatus.Approved,
                    CreatedAt = DateTimeOffset.UtcNow,
                    Breeder = null!
                });

            await db.SaveChangesAsync();

            var service = new LitterService(db, _notificationService.Object);

            var result = await service.GetLitters(
                new GetLittersRequest(LitterStatus.Approved, 1, 10),
                1,
                CancellationToken.None);

            Assert.Equal(2, result.Items.Count());
            Assert.Equal(2, result.TotalCount);

            Assert.All(result.Items, litter =>
            {
                Assert.Equal(1, litter.BreederId);
                Assert.Equal(LitterStatus.Approved, litter.Status);
            });
        }
    }
    
    [Fact]
    public async Task GetLitters_ShouldThrow_WhenBreederDoesNotExist()
    {
        var (db, connection) = await CreateDbContext();

        await using (db)
        await using (connection)
        {
            var service = new LitterService(db, _notificationService.Object);

            await Assert.ThrowsAsync<UnauthorizedException>(() =>
                service.GetLitters(
                    new GetLittersRequest(null, 1, 10),
                    1,
                    CancellationToken.None));
        }
    }
}