using Interfaces;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ScientificApp.RandomHistSerice.Model;

public class RangeResult : IEntity
{
    [BsonId, BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid Id { get; set; }
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public string Name { set; get; }
    public Guid SourceId { set; get; }
    public DateTime MomentAt { set; get; }
    public List<RangeResultItem> CalcResults { set; get; } = [];
}
