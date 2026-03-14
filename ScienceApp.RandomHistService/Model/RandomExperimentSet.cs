using Interfaces;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using ScienceApp.Dto;


namespace ScientificApp.RandomHistSerice.Model;

public class RandomExperimentSet : IEntity
{
    public required string AppName { set; get; } = string.Empty;
    [BsonDateTimeOptions(Kind = DateTimeKind.Local, Representation = BsonType.String)]
    public required DateTime Start { set; get; } = DateTime.Now;
    [BsonDateTimeOptions(Kind = DateTimeKind.Local, Representation = BsonType.String)]
    public required DateTime End { set; get; } = DateTime.Now;
    public required int MinValue { set; get; }
    public required int MaxValue { set; get; } = 1000;
    public required List<int> Result { set; get; } = [];
    [BsonId, BsonGuidRepresentation(GuidRepresentation.Standard)]
    public required Guid Id { set; get; } = Guid.NewGuid();
}

public static class RandomExperimentSetMapExtensions
{
    public static RandomExperimentSetDto MapToDto(this RandomExperimentSet model)
    {
        return new RandomExperimentSetDto
        {
            Id = model.Id,
            AppName = model.AppName,
            Start = model.Start,
            End = model.End,
            MinValue = model.MinValue,
            MaxValue = model.MaxValue,
            Result = model.Result
        };
    }

    public static RandomExperimentSet MapToModel(this RandomExperimentSetDto dto)
    {
        return new RandomExperimentSet
        {
            Id = dto.Id,
            AppName = dto.AppName,
            Start = dto.Start,
            End = dto.End,
            MinValue = dto.MinValue,
            MaxValue = dto.MaxValue,
            Result = dto.Result
        };
    }
}
