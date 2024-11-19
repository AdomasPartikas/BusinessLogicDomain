using BusinessLogicDomain.API.Services;
using BusinessLogicDomain.MarketDataDomainAPIClient;
using Hangfire;
using Hangfire.MemoryStorage;
using BusinessLogicDomain.API.Context.YouTradeDbContext;
using Microsoft.EntityFrameworkCore;
using BusinessLogicDomain.API.Profile;
using Newtonsoft.Json;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        policy =>
        {
            policy.WithOrigins("http://localhost:5173") 
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
});

builder.Services.AddControllers().AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
        options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
    });

builder.Services.AddDbContext<YouTradeContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("YouTradeDb"),
        new MySqlServerVersion(new Version(8, 0, 39))
    ).UseLazyLoadingProxies()
);

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "BusinessLogicDomain.API", Version = "v1" });
});

builder.Services.AddAutoMapper(typeof(AutoMapperProfile).Assembly);

builder.Services.AddHttpClient<MarketDataDomainClient>();
builder.Services.AddScoped<IMarketDataService, MarketDataService>();
builder.Services.AddScoped<IDbService, DbService>();
builder.Services.AddScoped<ITransactionService, TransactionService>();

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
    await marketDataService.RefreshMarketData();
}

app.UseHangfireDashboard();

#pragma warning disable CS0618 // Type or member is obsolete
app.UseHangfireServer();
#pragma warning restore CS0618 // Type or member is obsolete

RecurringJob.AddOrUpdate<IMarketDataService>(
    "refresh-market-data",
    service => service.RefreshMarketData(),
    "*/2 * * * *");

RecurringJob.AddOrUpdate<ITransactionService>(
    "refresh-user-transactions-portfolios",
    service => service.CreateIndividualJobs(),
    "*/2 * * * *");

app.UseCors("AllowSpecificOrigin");

app.UseRouting();
//app.UseHttpsRedirection(); // Commented out to allow for HTTP requests since Frontend is not using HTTPS

app.MapControllers();

app.UseSwagger();
app.UseSwaggerUI();

app.Run();
