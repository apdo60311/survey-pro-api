using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace survey_pro.Models;

public class SurveyResponse
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public string SurveyId { get; set; }

    public string RespondentId { get; set; }

    public DateTime SubmittedAt { get; set; }

    public Dictionary<string, object> Answers { get; set; } = new Dictionary<string, object>();
}