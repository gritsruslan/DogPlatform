using DogPlatform.API;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<DogPlatformDbContext>(options =>
    options.UseInMemoryDatabase("DogPlatformDb")
);

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.Run();
