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

    public string? RespondentId { get; set; }

    public DateTime SubmittedAt { get; set; }

    public List<QuestionResponse> Responses { get; set; }

}


public class QuestionResponse
{

    [BsonRepresentation(BsonType.ObjectId)]
    public string QuestionId { get; set; }
    public string Answer { get; set; }
    public List<string> SelectedOptions { get; set; }
}
