namespace ScienceApp.RandomExperimentApi.Interfaces.ApiSettings;

public class HangfireSettings
{
    public string DatabaseName { set; get; }
    public string JobId { set; get; }
    public string Cron { set; get; }
    public string Prefix { set; get; }
    public string ServerName { set; get; }
    public string RecurringJobId { set; get; }
}
