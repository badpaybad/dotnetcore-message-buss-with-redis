using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Confluent.Kafka;
using Confluent.Kafka.Serialization;
using Microsoft.AspNetCore.Mvc;
using RedisUsage.CqrsCore.Ef;

namespace ProjectSample.Apis.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KafkaTesterController : ControllerBase
    {
        [HttpPost]
        [Route("ClientSendData")]
        public async Task<object> ClientSendData(string topic, string msg)
        {
            var kafkaHost = ConfigurationManagerExtensions.GetValueByKey("Kafka:Host") ?? "127.0.0.1:9092";

            var config = new Dictionary<string, object>
                                      {
                                        { "bootstrap.servers",  kafkaHost },
                                        { "acks", "all" },
                {"retries",3 }
                                      };

            Console.WriteLine("Push to Kafka");
            var sw = Stopwatch.StartNew();
            using (var producer = new Producer<Null, string>(config, null, new StringSerializer(Encoding.UTF8)))
            {
                var result = await producer.ProduceAsync(topic, null, msg);

                producer.Flush(1000);
            }
            sw.Stop();
            Console.WriteLine($"Pushed into Kafka MSG: '{msg}' to TOPIC: {topic} in miliseconds: {sw.ElapsedMilliseconds}");

            return new
            {
                Success = true,
                Topic = topic
            };
        }

    }
}
