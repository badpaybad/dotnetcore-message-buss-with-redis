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
            string text = msg;

            var config = new Dictionary<string, object>
                                      {
                                        { "bootstrap.servers",  ConfigurationManagerExtensions.GetValueByKey("Kafka:Host") ?? "127.0.0.1:2181" },
                                        { "acks", "all" },
                {"retries",3 }
                                      };

            Console.WriteLine("Push to Kafka");
            var sw = Stopwatch.StartNew();
            using (var producer = new Producer<Null, string>(config, null, new StringSerializer(Encoding.UTF8)))
            {
                var result = await producer.ProduceAsync(topic, null, text);

                producer.Flush(1000);
            }
            sw.Stop();
            Console.WriteLine($"Pushed MSG: {text} to TOPIC: {topic} into Kafka in miliseconds: {sw.ElapsedMilliseconds}");

            return new
            {
                Success = true,
                Topic = topic
            };
        }

    }
}
