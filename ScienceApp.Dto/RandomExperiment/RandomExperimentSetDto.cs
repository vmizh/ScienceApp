namespace ScienceApp.Dto.RandomExperiment
{
    public record RandomExperimentSetDto
    {
        public required Guid Id { set; get; }
        public required string AppName { set; get; }
        public required DateTime Start { set; get; }
        public required DateTime End { set; get; }
        public required int MinValue { set; get; }
        public required int MaxValue { set; get; }
        public required List<int> Result { set; get; } = [];
    }
}
