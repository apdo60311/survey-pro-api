using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace survey_pro.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("username")]
        public required string Username { get; set; }

        [BsonElement("email")]
        public required string Email { get; set; }

        [BsonElement("passwordHash")]
        public required string PasswordHash { get; set; }

        [BsonElement("roles")]
        public List<string> Roles { get; set; } = new List<string> { "User" };

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; }

        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; }

        [BsonIgnore]
        public DateTime CreationTime => IdAsObjectId.CreationTime;

        private ObjectId IdAsObjectId => string.IsNullOrEmpty(Id) ? ObjectId.Empty : ObjectId.Parse(Id);

    }
}
