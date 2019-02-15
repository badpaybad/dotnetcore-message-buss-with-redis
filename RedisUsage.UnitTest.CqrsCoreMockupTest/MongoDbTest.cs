using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using Newtonsoft.Json;
using RedisUsage.CqrsCore.Ef;
using System;
using System.Collections.Generic;
using System.Text;

namespace RedisUsage.UnitTest.CqrsCoreMockupTest
{
    public class MyMongoDbContext : IDisposable
    {
        IMongoClient client;
        IMongoDatabase database;

        //https://docs.microsoft.com/en-us/aspnet/core/tutorials/first-mongo-app?view=aspnetcore-2.2&tabs=visual-studio

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectionString">mongodb://localhost:27017</param>
        /// <param name="dbName">BookstoreDb</param>
        public MyMongoDbContext(string connectionString, string dbName)
        {
            client = new MongoClient(connectionString);
            database = client.GetDatabase(dbName);

            match_datas = database.GetCollection<match_data>("match_data");

        }

        public IMongoCollection<match_data> match_datas { get; set; }

        public void Dispose()
        {
        }
    }

    public class match_data
    {
        [BsonId]
        //[BsonRepresentation(BsonType.ObjectId)]
        public int id { get; set; }

        [BsonElement("home_team")]
        public string home_team { get; set; }

        [BsonElement("away_team")]
        public string away_team { get; set; }

        [BsonElement("tournament_name")]
        public string tournament_name { get; set; }

        [BsonElement("site")]
        public string site { get; set; }

        [BsonElement("match_date")]
        public string match_date { get; set; }

        [BsonElement("match_time")]
        public int match_time { get; set; }

        [BsonElement("sporttype")]
        public string sporttype { get; set; }

        [BsonElement("have_live")]
        public string have_live { get; set; }
    }

    [TestClass]
    public class MongoDbTest
    {
        [TestMethod]
        public void Read()
        {
            //connection string mongodb://localhost:27017
            //
            using (var db = new MyMongoDbContext("mongodb://localhost:27017/odds_total", "odds_total"))
            {
                var itm = db.match_datas.Find(i => true).FirstOrDefault();

                string value = JsonConvert.SerializeObject(itm);

                itm.have_live = "No";
                db.match_datas.ReplaceOne(i => i.id == itm.id, itm);


                Console.WriteLine(value);
            }
        }
    }
}
