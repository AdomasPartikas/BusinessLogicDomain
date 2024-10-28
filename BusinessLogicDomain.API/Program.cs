using BusinessLogicDomain.API.Services;
using BusinessLogicDomain.MarketDataDomainAPIClient;
using Hangfire;
using Hangfire.MemoryStorage;
using BusinessLogicDomain.API.Context.YouTradeDbContext;
using Microsoft.EntityFrameworkCore;
using BusinessLogicDomain.API.Profile;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddDbContext<YouTradeContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("YouTradeDb"),
        new MySqlServerVersion(new Version(8, 0, 39))
    )
);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        policy =>
        {
            policy.WithOrigins("https://our-very-friendly-frontend-url") 
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "BusinessLogicDomain.API", Version = "v1" });
});

builder.Services.AddAutoMapper(typeof(AutoMapperProfile).Assembly);

builder.Services.AddHttpClient<MarketDataDomainClient>();
builder.Services.AddScoped<IMarketDataService, MarketDataService>();
builder.Services.AddScoped<IDbService, DbService>();

builder.Services.AddHangfire(config =>
{
    config.UseMemoryStorage();
});

builder.Services.AddHangfireServer();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<YouTradeContext>();
    dbContext.Database.EnsureCreated();

    var marketDataService = scope.ServiceProvider.GetRequiredService<IMarketDataService>();
    await marketDataService.RetrieveAndSaveAvailableCompanies();
}

app.UseHangfireDashboard();

#pragma warning disable CS0618 // Type or member is obsolete
app.UseHangfireServer();
#pragma warning restore CS0618 // Type or member is obsolete

RecurringJob.AddOrUpdate<IMarketDataService>(
    "refresh-market-data",
    service => service.RefreshMarketData(),
    "*/1 * * * *");

app.UseRouting();
app.UseHttpsRedirection();

app.UseCors("AllowSpecificOrigin");

app.MapControllers();

app.UseSwagger();
app.UseSwaggerUI();

app.Run();
