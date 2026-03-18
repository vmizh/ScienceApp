using MangoDbBaseRepository;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using ScienceApp.Dto.RandomExperiment;
using ScienceApp.RandomExperimentApi.Interfaces.ApiSettings;
using ScientificApp.RandomHistSerice.Model;
using ScientificApp.RandomHistService.Repositories.RandomExperimentSetRepository;

namespace ScientificApp.RandomHistService.Repositories.RangeResultRepository;

public class RangeResultRepository : MongoBaseRepository<RangeResult>, IRangeResultRepository
{
    private IRandomExperimentSetRepository _ExperimentRepository;

    public RangeResultRepository(IMongoClient client, IOptions<ExperimentSettings> eSettings,
        ILogger<MongoBaseRepository<RangeResult>> logger,
        IRandomExperimentSetRepository experimentRepository) : base(client, logger)
    {
        _ExperimentRepository = experimentRepository;
        DatabaseName = eSettings.Value.DatabaseName;
        CollectionName = eSettings.Value.RangeResultCollectionName;
    }

    public async Task<int> CalcRangeResult(RandomCalcOptionsDto options)
    {
        return 1;
    }
}
