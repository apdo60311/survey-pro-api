using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace survey_pro.Models;

public class Question
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public required QuestionType Type { get; set; }
    public List<string> Options { get; set; } = new List<string>();
    public string? ImageUrl { get; set; }

    public bool IsRequired { get; set; } = true;
}

public enum QuestionType
{
    MultipleChoice,
    SingleChoice,
    Text,
    Rating
}