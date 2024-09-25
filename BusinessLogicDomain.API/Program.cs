using BusinessLogicDomain.API.Services;
using BusinessLogicDomain.MarketDataDomainAPIClient;
using Hangfire;
using Hangfire.MemoryStorage;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "BusinessLogicDomain.API", Version = "v1" });
});

builder.Services.AddHttpClient<MarketDataDomainClient>();
builder.Services.AddSingleton<IMarketDataService, MarketDataService>();

builder.Services.AddHangfire(config =>
{
    config.UseMemoryStorage();
});

builder.Services.AddHangfireServer();

var app = builder.Build();

app.UseHangfireDashboard();

#pragma warning disable CS0618 // Type or member is obsolete
app.UseHangfireServer();
#pragma warning restore CS0618 // Type or member is obsolete

RecurringJob.AddOrUpdate<IMarketDataService>(
    "refresh-stock-symbols",
    service => service.GetMarketData(),
    "*/1 * * * *");

app.UseRouting();
app.UseHttpsRedirection();
app.MapControllers();

app.UseSwagger();
app.UseSwaggerUI();

app.Run();
