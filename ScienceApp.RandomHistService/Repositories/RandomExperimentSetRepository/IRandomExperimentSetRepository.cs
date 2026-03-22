using ScienceApp.Dto.RandomExperiment;
using ScientificApp.RandomHistSerice.Model;

namespace ScientificApp.RandomHistService.Repositories.RandomExperimentSetRepository;

public interface IRandomExperimentSetRepository
{
    Task<Guid> CalcExperiment();
    Task<Guid> CalcExperiment(SendExperimentOptionsDto opt);
    Task<List<RandomExperimentSet>> GetRangeResults(DateTime startDate, DateTime endDate);
    Task<int> CalcAggregate(DateTime start, DateTime end, int rangeValue);
    Task<List<Guid>> GetIdsForDateTimeRange(DateTime start, DateTime end);



}
