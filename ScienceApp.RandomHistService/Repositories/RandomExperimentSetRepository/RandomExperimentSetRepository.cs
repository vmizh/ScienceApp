using MangoDbBaseRepository;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using ScienceApp.RandomExperimentApi.Interfaces.ApiSettings;
using ScientificApp.RandomHistSerice.Model;
using ScientificApp.RandomHistService.CalcExperiment;

namespace ScientificApp.RandomHistService.Repositories.RandomExperimentSetRepository;

public class RandomExperimentSetRepository : MongoBaseRepository<RandomExperimentSet>, IRandomExperimentSetRepository
{
    private readonly string _appName = "CharpApp";
    private readonly int _countCalc = 10000;
    private readonly int _maxVal = 1000;
    private readonly int _minVal = 1;

    public RandomExperimentSetRepository(IMongoClient client, IOptions<ExperimentSettings> eSettings,
        ILogger<MongoBaseRepository<RandomExperimentSet>> logger) : base(client, logger)
    {
        _countCalc = eSettings.Value.CountCalc;
        _minVal = eSettings.Value.MinimalValue;
        _maxVal = eSettings.Value.MaximalValue;
        DatabaseName = eSettings.Value.DatabaseName;
        CollectionName = eSettings.Value.CollectionName;
    }

    public void CalcExperiment()
    {
        Task.Run(() => CreateAsync(RandomCalc.Calc(_appName, _countCalc, _minVal, _maxVal)));
    }
}
