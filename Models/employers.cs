using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace server.Models
{
    public class employers
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("mail")]
        public string mail { get; set; }
        [BsonElement("phone")]
        public string phone { get; set; }

        [BsonElement("address")]
        public string address { get; set; }
        [BsonElement("first_name")]
        public string first_name { get; set; }
        [BsonElement("last_name")]
        public string last_name { get; set; }        [BsonElement("id_manager")] // מוודא שמונגו יודע לקשר את זה
        public string IdManager { get; set; }
    }
}
