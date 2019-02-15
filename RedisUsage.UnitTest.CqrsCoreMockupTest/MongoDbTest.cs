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


            SampleMongoDocs = database.GetCollection<SampleMongoDoc>("SampleMongoDoc");

        }

        public IMongoCollection<match_data> match_datas { get; set; }

        public IMongoCollection<SampleMongoDoc> SampleMongoDocs { get; set; }

        public void Dispose()
        {
        }
    }

    public class SampleMongoDoc
    {
        [BsonId]
        public Guid Id { get; set; }

        public string SampleName { get; set; }
        
        [BsonElement("SampleVersion")]
        public int? SampleVersion { get; set; }

    }

    public class match_data
    {
        [BsonId]
        [BsonRepresentation(BsonType.Int32)]
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

        public Guid UniqueId { get; set; }
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
                var itm = db.match_datas.Find(i => i.id== 322585665).FirstOrDefault();

                string value = JsonConvert.SerializeObject(itm);

                itm.have_live = "No";
                itm.UniqueId = Guid.NewGuid();
                db.match_datas.ReplaceOne(i => i.id == itm.id, itm);


                Console.WriteLine(value);

                db.SampleMongoDocs.InsertOne(new SampleMongoDoc()
                {
                    Id = Guid.NewGuid(),
                    //SampleName = "Version 1",
                    SampleVersion = null
                });

                //var sall = db.SampleMongoDocs.Find(i => true).ToList();

                Guid id1 = Guid.Parse("28fa464c-b121-4c56-929b-af4e746e8802");
                var s1 = db.SampleMongoDocs.Find(i => i.Id == id1)
                    .FirstOrDefault();

                //Guid id2 = Guid.Parse("9c5878eb-7e0a-424d-9ffe-371923fdfb90");
                //var s2 = db.SampleMongoDocs.Find(i => i.Id == id2)
                //  .FirstOrDefault();
            }
        }
    }
}
