namespace ScienceApp.RandomExperimentApi.Interfaces.ApiSettings;

public class AppSettings
{
    public string ApplicationName { get; set; }
    public int MaxRetries { get; set; }
    public bool EnableLogging { get; set; }
    public Database Database { get; set; }
    public string MongoDBClient { set; get; }

    public HangfireSettings HangfireSettings { get; set; }
    public ExperimentSettings ExperimentSettings { get; set; }

    public Logging Logging { get; set; }
    public string AllowedHosts { get; set; }
    public string mongoConnectionString { set; get; }
}
