using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace survey_pro.Models;

public class Survey
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    public required string Title { get; set; }
    public string? Description { get; set; }
    public List<string> Categories { get; set; } = [];
    public int NumberOfQuestions { get; set; }
    public TimeSpan EstimatedCompletionTime { get; set; }

    public string? CoverImageUrl { get; set; }
    public string? CreatedBy { get; set; } = "unanonymous";

    public List<Question>? Questions { get; set; } = new List<Question>();
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

}