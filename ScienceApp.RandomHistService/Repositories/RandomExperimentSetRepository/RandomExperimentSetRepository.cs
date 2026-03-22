using CommonHelper.Paging;
using MangoDbBaseRepository;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using ScienceApp.Dto.RandomExperiment;
using ScienceApp.RandomExperimentApi.Interfaces.ApiSettings;
using ScientificApp.RandomHistSerice.Model;
using ScientificApp.RandomHistService.CalcExperiment;
using ScientificApp.RandomHistService.Helper;

namespace ScientificApp.RandomHistService.Repositories.RandomExperimentSetRepository;

public class RandomExperimentSetRepository : MongoBaseRepository<RandomExperimentSet>, IRandomExperimentSetRepository
{
    private readonly string _appName = "CharpApp";
    private readonly int _countCalc = 10000;
    private readonly int _maxVal = 1000;
    private readonly int _minVal = 1;
    private readonly int _rangeValue = 10;

    public RandomExperimentSetRepository(IMongoClient client, IOptions<ExperimentSettings> eSettings,
        ILogger<MongoBaseRepository<RandomExperimentSet>> logger) : base(client, logger)
    {
        _countCalc = eSettings.Value.CountCalc;
        _minVal = eSettings.Value.MinimalValue;
        _maxVal = eSettings.Value.MaximalValue;
        _rangeValue = eSettings.Value.RangeValue;
        DatabaseName = eSettings.Value.DatabaseName;
        CollectionName = eSettings.Value.CollectionName;
    }

    private List<RangeResultItem> getAggregateResult(List<int> data, int minValue, int maxValue, int rangeValue)
    {
        var res = new List<RangeResultItem>();
        var d = RangeSplitter.SplitRange(minValue, maxValue, rangeValue).OrderBy(_ => _.Start).ToArray();
        for (var i = 0; i < d.Length; i++)
        {
            var newItem = new RangeResultItem
            {
                RangeNumber = i + 1,
                UpperLimit = d[i].End,
                LowerLimit = d[i].Start,
                Count = data.Count(_ => _ >= d[i].Start && _ <= d[i].End)
            };
            res.Add(newItem);
        }

        return res;
    }

    public async Task<Guid> CalcExperiment()
    {
        var newItem = RandomCalc.Calc(_appName, _countCalc, _minVal, _maxVal);
        newItem.CalcRange10Results = getAggregateResult(newItem.Result, _minVal, _maxVal, 10);
        var res = await CreateAsync(newItem);
        return res.Id;
    }

    public async Task<Guid> CalcExperiment(SendExperimentOptionsDto opt)
    {
        var newItem = RandomCalc.Calc(opt.AppName, opt.Count, opt.MinVal, opt.MaxVal);
        newItem.CalcRange10Results = getAggregateResult(newItem.Result, opt.MinVal, opt.MaxVal, opt.RangeValue);
        var res = await CreateAsync(newItem);
        return res.Id;
    }

    public async Task<List<RandomExperimentSet>> GetRangeResults(DateTime startDate, DateTime endDate)
    {
        var filter = Builders<RandomExperimentSet>.Filter.And(
            Builders<RandomExperimentSet>.Filter.Gte(x => x.Start, startDate.ToLocalTime()), // >= fromDate
            Builders<RandomExperimentSet>.Filter.Lte(x => x.Start, endDate.ToLocalTime()) // <= toDate
        );
        var q = Collection.Find(filter);
        var result = await Collection.Find(filter).ToListAsync();
        return result;
    }

    public async Task<int> CalcAggregate(DateTime start, DateTime end, int rangeValue)
    {
        var ids = await GetIdsForDateTimeRange(start, end);
        ids.ForEachPage(pageSize: 20, (pageItems, pageNumber) =>
        {
            var filter = Builders<RandomExperimentSet>.Filter.In(p => p.Id, ids);
            var listData = Collection.Find(filter).ToList();

        });

        var filter = Builders<RandomExperimentSet>.Filter.In(p => p.Id, ids);
        var data1 = Collection.Find(filter).ToList();


        var data = await GetRangeResults(start, end);
        var items = data.Where(_ => _.CalcRange10Results is null).ToList();
        var res = items.Count;
        if (res <= 0) return res;
        foreach (var item in items)
        {
            item.CalcRange10Results = getAggregateResult(item.Result, item.MinValue, item.MaxValue, _rangeValue);
            await UpdateAsync(item);
        }

        return res;
    }

    public async Task<List<Guid>> GetIdsForDateTimeRange(DateTime start, DateTime end)
    {
        var filter = Builders<RandomExperimentSet>.Filter.And(
            Builders<RandomExperimentSet>.Filter.Gte(x => x.Start, start.ToLocalTime()), // >= fromDate
            Builders<RandomExperimentSet>.Filter.Lte(x => x.Start, end.ToLocalTime()) // <= toDate
        );
        var q = Collection.Find(filter);
        var result = await Collection.Find(filter).Project(_ => _.Id).ToListAsync();
        return result;
    }
}
