using HackerNewsCachingService;
using Microsoft.Extensions.Caching.Memory;

var builder = WebApplication.CreateSlimBuilder(args);
builder.Services.AddHostedService<CacheUpdaterService>();
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMemoryCache();

builder.Services.Configure<CacheServerConfiguration>(
    builder.Configuration.GetSection(nameof(CacheServerConfiguration)));
builder.Services.AddTransient<HackerNewsClient>();
builder.Services.AddSingleton<BestStoriesReadModel>();
    

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.Run();