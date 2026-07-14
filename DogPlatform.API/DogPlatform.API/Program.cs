using DogPlatform.API;
using DogPlatform.API.Middlewares;
using DogPlatform.API.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddEndpointsApiExplorer()
    .AddSwaggerGen()
    .AddControllers();

builder.Services.AddDbContext<DogPlatformDbContext>(options =>
    options.UseInMemoryDatabase("DogPlatformDb")
);

builder.Services
    .AddSingleton<INotificationService, ConsoleNotificationService>()
    .AddScoped<ILitterService, LitterService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<DogPlatformDbContext>();
    await DatabaseSeeder.SeedAsync(dbContext);
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.MapControllers();

app.UseSwagger()
    .UseSwaggerUI();

app.Run();
