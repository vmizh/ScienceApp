using ScienceApp.Dto.RandomExperiment;

namespace ScientificApp.RandomHistService.Repositories.RangeResultRepository;

public interface IRangeResultRepository
{
    Task<int> CalcRangeResult(RandomCalcOptionsDto options);

}
