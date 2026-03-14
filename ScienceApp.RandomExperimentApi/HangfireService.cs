using Hangfire;
using Hangfire.Console;
using Hangfire.Mongo;
using Hangfire.Mongo.Migration.Strategies;
using Hangfire.Mongo.Migration.Strategies.Backup;
using MongoDB.Driver;
using ScienceApp.RandomExperimentApi.Interfaces.ApiSettings;

namespace ScientificApp.ApiService;

public static class HangfireService
{
    public static void ConfigureHangfireService(this WebApplicationBuilder builder)
    {
        var cfgBuilder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory());

        cfgBuilder
            .AddJsonFile("app.json", true, true)
            .AddJsonFile($"appsettings.json", true, true)
            .AddEnvironmentVariables();

        var configuration = cfgBuilder.Build();

        var appSettings = configuration.Get<AppSettings>();

        //var mongoUrlBuilder = new MongoUrlBuilder(builder.Configuration.GetValue<string>("ConnectionStrings:MongoDb"));
        var mongoClient = new MongoClient(appSettings.Database.ConnectionString);

        builder.Services.AddHangfire(configuration => configuration
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseConsole()
            .UseMongoStorage(mongoClient, appSettings.HangfireSettings.DatabaseName, new MongoStorageOptions
            {
                MigrationOptions = new MongoMigrationOptions
                {
                    MigrationStrategy = new MigrateMongoMigrationStrategy(),
                    BackupStrategy = new CollectionMongoBackupStrategy()
                },
                CheckQueuedJobsStrategy = CheckQueuedJobsStrategy.TailNotificationsCollection,
                Prefix = appSettings.HangfireSettings.Prefix,
                CheckConnection = false
            })
        );

        builder.Services.AddHangfireServer(serverOptions =>
        {
            serverOptions.ServerName = appSettings.HangfireSettings.ServerName;
        });
    }
}
