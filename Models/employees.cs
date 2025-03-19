using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication14.Models
{
    public class employees
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        [BsonElement("password")]
        public string password { get; set; }

        [BsonElement("mail")]
        public string mail { get; set; }
        [BsonElement("first_name")]
        public string first_name { get; set; }
        [BsonElement("last_name")]
        public string last_name { get; set; }

        [BsonElement("birth_date")]
        public string birth_date { get; set; }
        [BsonElement("phone")]
        public string phone { get; set; }
        [BsonElement("resume")]
        public string resume { get; set; }
      
  
 
    


}


}
