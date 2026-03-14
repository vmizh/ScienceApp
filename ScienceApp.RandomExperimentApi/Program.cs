using Hangfire;
using MangoDbBaseRepository;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using ScienceApp.RandomExperimentApi;
using ScienceApp.RandomExperimentApi.Interfaces.ApiSettings;
using ScientificApp.ApiService;
using ScientificApp.RandomHistSerice.Model;
using ScientificApp.RandomHistService;
using ScientificApp.RandomHistService.Repositories.RandomExperimentSetRepository;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

builder.Configuration
    .AddJsonFile("app.json", true, true)
    .AddJsonFile("appsettings.json", true, true)
    .AddEnvironmentVariables();

builder.Services.AddOptions();
builder.Services.Configure<ExperimentSettings>(builder.Configuration.GetSection("ExperimentSettings"));
var cfgBuilder = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory());

cfgBuilder
    .AddJsonFile("app.json", true, true)
    .AddJsonFile("appsettings.json", true, true)
    .AddEnvironmentVariables();

var configuration = cfgBuilder.Build();

var appSettings = configuration.Get<AppSettings>();

builder.Services.AddOpenApi();

Debug.Assert(appSettings != null, nameof(appSettings) + " != null");
BsonSerializer.RegisterSerializer(DateTimeSerializer.LocalInstance);
Console.WriteLine($"Тест коннект - {appSettings.Database.ConnectionString}");

builder.Services.AddSingleton<IMongoClient>(sp => new MongoClient(appSettings.Database.ConnectionString));

builder.ConfigureHangfireService();

builder.Services.AddRandomHistServiceDependencies();

var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
//app.UseExceptionHandler();
app.UseHangfireDashboard();
app.ConfigureJobs();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", async ([FromServices] IRandomExperimentSetRepository repository, ILogger<RandomExperimentSet> logger) =>
{
    await ((MongoBaseRepository<RandomExperimentSet>)repository).CreateAsync(new RandomExperimentSet
    {
        Id = Guid.NewGuid(),
        AppName = "TestApp",
        Start = DateTime.Now,
        End = DateTime.Now,
        MinValue = 0,
        MaxValue = 1000,
        Result = [1, 2, 3, 35, 5]
    });
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.Run();

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
