using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Text.Json;
using WebApplication14.Models;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc;
using ZstdSharp.Unsafe;

namespace WebApplication14
{

    public class MongoDBManager<T>
    {
        private readonly IMongoCollection<T> _collection;
        private readonly IMongoDatabase _database;
        string connection_string;
        private readonly IConfiguration _configuration;
        public MongoDBManager(IConfiguration configuration, string collectionName)
        {



            connection_string = Environment.GetEnvironmentVariable("ConnectionStrings__mongodb");
            var client = new MongoClient(connection_string);
            _database = client.GetDatabase(Environment.GetEnvironmentVariable("database"));

            _collection = _database.GetCollection<T>(collectionName);
        }
        // Insert a document
        public async Task InsertAsync([FromBody] T document)
        {
            await _collection.InsertOneAsync(document);
        }

        // Update a document based on a filter
        public async Task UpdateAsync(string mail, T updatedObject)
        {
            var filter = Builders<T>.Filter.Eq("mail", mail);
            await _collection.ReplaceOneAsync(filter, updatedObject);
        }
        // Update a document based on a filter
        public async Task UpdateFieldAsync_ById(string _id, string fieldName, object newValue)
        {
            var filter = Builders<T>.Filter.Eq("_id", ObjectId.Parse(_id));
            var update = Builders<T>.Update.Set(fieldName, newValue);
            await _collection.UpdateOneAsync(filter, update);
        }
        public async Task<DeleteResult> DeleteByIdAsync(

            string id)
        {
            // Ensure that the provided id is a valid ObjectId
            if (!ObjectId.TryParse(id, out var objectId))
            {
                throw new ArgumentException("Invalid ObjectId format");
            }

            // Build the filter to match the document by its _id field
            var filter = Builders<T>.Filter.Eq("_id", objectId);

            // Execute the delete operation
            return await _collection.DeleteOneAsync(filter);
        }
        // Delete a document based on a filter
        //public async Task DeleteAsync(Expression<Func<T, bool>> filter)
        //{
        //    await _collection.DeleteOneAsync(filter);
        //}

        // Query documents with a filter
        public async Task<List<T>> QueryAsync(Expression<Func<T, bool>> filter)
        {
            return await _collection.Find(filter).ToListAsync();
        }
        public IMongoCollection<T> GetCollectionByName<T>() where T : class
        {
            return (IMongoCollection<T>)_collection;
        }
        public async Task<T> QueryByIdAsync(string id)
        {
            var filter = Builders<T>.Filter.Eq("_id", new ObjectId(id));
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }
        public async Task<T> QueryBymailAsync(string mail, string pass)
        {
            var filter = Builders<T>.Filter.And(
    Builders<T>.Filter.Eq("mail", mail),
    Builders<T>.Filter.Eq("password", pass)
);

            return await _collection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<T> QueryBymailOnlyAsync(string mail)
        {
            var filter = Builders<T>.Filter.Eq("mail", mail);

            return await _collection.Find(filter).FirstOrDefaultAsync();
        }


    }
}


