using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace survey_pro.Models;

public class Role
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    [BsonElement("name")]
    public string Name { get; set; }

    [BsonElement("description")]
    public string Description { get; set; }
}
