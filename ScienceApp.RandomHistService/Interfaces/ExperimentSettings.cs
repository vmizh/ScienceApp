namespace ScienceApp.RandomExperimentApi.Interfaces.ApiSettings;

public class ExperimentSettings
{
    public string DatabaseName { set; get; }
    public string CollectionName { set; get; }
    public string RangeResultCollectionName { set; get; }

    public int CountCalc { set; get; }
    public int MinimalValue { set; get; }
    public int MaximalValue { set; get; }
    public int RangeValue { set; get; }
}
