namespace ScienceApp.Dto.RandomExperiment;

public record SendExperimentOptionsDto
{
    public required string AppName { init; get; }
    public required int Count { init; get; } = 10000;
    public required int MinVal { init; get; } = 1;
    public required int MaxVal { init; get; } = 1000;
    public required int RangeValue { set; get; } = 10;

};
