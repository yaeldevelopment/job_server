using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace server.Models
{
    public class Jobs
    {


        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        [BsonElement("title")]
        public string title { get; set; }

        [BsonElement("componay")]
        public string componay { get; set; }
        [BsonElement("publication_date")]
        public string publication_date { get; set; }
        [BsonElement("job_location")]
        public string[] job_location { get; set; }
        [BsonElement("job_type")]
        public string job_type { get; set; }
        [BsonElement("salary")]
        public string salary { get; set; }
        [BsonElement("additional_conditions")]
        public string additional_conditions { get; set; }
        [BsonElement("html_word")]
        public string html_word { get; set; }
        [BsonElement("employer")]
        public string employer { get; set; }

                    [BsonElement("employees_send")]
        public List<string> employees_send { get; set; }
    }
}
