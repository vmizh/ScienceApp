using Hangfire;
using ScienceApp.RandomExperimentApi.Interfaces.ApiSettings;
using ScientificApp.RandomHistService.Repositories.RandomExperimentSetRepository;

namespace ScienceApp.RandomExperimentApi;

public static class HangfireJobConfiguration
{
    public static void ConfigureJobs(this WebApplication app)
    {
        var cfgBuilder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory());

        cfgBuilder
            .AddJsonFile("app.json", true, true)
            .AddJsonFile($"appsettings.json", true, true)
            .AddEnvironmentVariables();

        var configuration = cfgBuilder.Build();

        var appSettings = configuration.Get<AppSettings>();
        app.Services.CreateScope();

        RecurringJob.AddOrUpdate<IRandomExperimentSetRepository>(recurringJobId: appSettings.HangfireSettings.RecurringJobId,
            methodCall: j => j.CalcExperiment(),
            appSettings.HangfireSettings.Cron);
    }
}
