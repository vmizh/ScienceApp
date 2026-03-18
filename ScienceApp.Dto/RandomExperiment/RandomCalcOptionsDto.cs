namespace ScienceApp.Dto.RandomExperiment;

public record RandomCalcOptionsDto
{
    public string CalcName { set; get; }
    public DateTime? Start { set; get; }
    public DateTime? End { set; get; }
    public int RangeValue { set; get; }
    public Guid? ExperimentId { set; get; }

}
