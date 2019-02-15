using Confluent.Kafka;
using Confluent.Kafka.Serialization;
using MQTTnet;
using MQTTnet.Protocol;
using MQTTnet.Server;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using RedisUsage.CqrsCore.Ef;
using System.Net.Http;
using System.Security.Cryptography;
using System.Linq;

namespace Mqtt.MqttServerBrocker
{
    class Program
    {
        static MqttFactory _mqttfactory = new MqttFactory();
        static IMqttServer _mqttServer;
        static string _kafkaHost = "127.0.0.1:9092";
        static Dictionary<string, object> _mqttConfig = new Dictionary<string, object>();

        public static void Main(string[] args)
        {
            TestMovoHubAuth();
            Console.ReadLine();
            return;

            var redisHost = ConfigurationManagerExtensions.GetValueByKey("Redis:Host") ?? "127.0.0.1";
            var redisPort = ConfigurationManagerExtensions.GetValueByKey("Redis:Port") ?? "6379";
            var redisPwd = ConfigurationManagerExtensions.GetValueByKey("Redis:Password") ?? string.Empty;
            int? redisPortInt = null;
            if (!string.IsNullOrEmpty(redisPort))
            {
                redisPortInt = int.Parse(redisPort);
            }

            RedisUsage.RedisServices.RedisServices.Init(redisHost, redisPortInt, redisPwd);

            _kafkaHost = ConfigurationManagerExtensions.GetValueByKey("Kafka:Host") ?? "127.0.0.1:9092";
            var mqttHost = ConfigurationManagerExtensions.GetValueByKey("Mqtt:Host") ?? "127.0.0.1";

            _mqttConfig = new Dictionary<string, object>
                                      {
                                       { "bootstrap.servers", mqttHost },
                                        { "acks", "all" }
                                      };

            _mqttServer = _mqttfactory.CreateMqttServer();
            _mqttServer.ClientConnected += _mqttServer_ClientConnected;
            _mqttServer.ClientDisconnected += _mqttServer_ClientDisconnected;
            _mqttServer.ClientSubscribedTopic += _mqttServer_ClientSubscribedTopic;
            _mqttServer.ClientUnsubscribedTopic += _mqttServer_ClientUnsubscribedTopic;
            _mqttServer.ApplicationMessageReceived += _mqttServer_ApplicationMessageReceived;

            MqttServerOptions options = BuildMqttServerOptions();

            var tserver = _mqttServer.StartAsync(options);
            tserver.Wait();

            Console.WriteLine("server started with mqttConfig: " + JsonConvert.SerializeObject(_mqttConfig));
            Console.WriteLine("server started with kafkaConfig: " + JsonConvert.SerializeObject(_kafkaHost));

            while (true)
            {
                Console.WriteLine("--------------");
                Console.WriteLine("Type 'quit' to quit");

                var cmd = Console.ReadLine();

                if (cmd == "quit")
                {
                    var tserverStop = _mqttServer.StopAsync();
                    tserver.Wait();

                    Environment.Exit(0);
                    return;
                }
                if (cmd == "kafka-test")
                {
                    TestPushDataToKafka();
                }
                if (cmd.StartsWith("s-p"))
                {
                    try
                    {
                        var clientId = cmd.Substring("s-p".Length + 1).Trim();

                        _mqttServer.PublishAsync(new MqttApplicationMessageBuilder()
                            .WithTopic(clientId)
                            .WithPayload("Ok Halo client")
                            .WithAtLeastOnceQoS()
                            .Build()
                            );
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }

            }

        }

        private static void TestMovoHubAuth()
        {
            //https://butaneko.sakura.ne.jp/auth/

            var url = "https://butaneko.sakura.ne.jp";
           
            using (var c = new HttpClient())
            {
                c.BaseAddress = new Uri(url+"/auth/");

                c.DefaultRequestHeaders.Clear();

                HttpRequestMessage _1stRequest = new HttpRequestMessage(HttpMethod.Get, c.BaseAddress);
                _1stRequest.Content = new StringContent("", Encoding.UTF8, "text/plain");

                var _1stResponse = c.SendAsync(_1stRequest).GetAwaiter().GetResult();

              
                var realm = _1stResponse.Headers.GetValues("WWW-Authentication").ToList().FirstOrDefault();

                var arrRealm = realm.Split(new[] { '\"' });
                if (arrRealm.Length > 1)
                {
                    realm = arrRealm[1];
                }
                else
                {
                    realm = "duks-tacho";
                }
                
                var simId = "T8981200017251102088";
                // simId = "1200017251102088";
                // simId = "8981200017251102088";
                simId = "sora";
                var digest = GetMd5(realm + ":" + simId);

                if ((int)_1stResponse.StatusCode == 401)
                {

                    using (var c1 = new HttpClient())
                    {
                        c1.BaseAddress = c.BaseAddress;

                        c1.DefaultRequestHeaders.Clear();

                        c1.DefaultRequestHeaders.Add("Authorization", "Basic " + digest);

                        HttpRequestMessage msgRequest1 = new HttpRequestMessage(HttpMethod.Get, c.BaseAddress);
                        msgRequest1.Content = new StringContent("", Encoding.UTF8, "text/plain");

                        var response1 = c.SendAsync(msgRequest1).GetAwaiter().GetResult();

                        string responsJson = JsonConvert.SerializeObject(response1);

                        Console.WriteLine(responsJson);
                    }
                }
            }
        }

        static string GetMd5(string input)
        {
            MD5 md5Hash = MD5.Create();
            //
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();

        }

        private static MqttServerOptions BuildMqttServerOptions()
        {
            var options = new MqttServerOptions
            {
                ConnectionValidator = p =>
                {
                    //if (p.ClientId == "SpecialClient")
                    //{
                    //    if (p.Username != "USER" || p.Password != "PASS")
                    //    {
                    //        p.ReturnCode = MqttConnectReturnCode. ConnectionRefusedBadUsernameOrPassword;
                    //    }
                    //}
                },

                Storage = new RetainedMessageHandler(),

                ApplicationMessageInterceptor = context =>
                {
                    //if (MqttTopicFilterComparer.IsMatch(context.ApplicationMessage.Topic, "/myTopic/WithTimestamp/#"))
                    //{
                    //    // Replace the payload with the timestamp. But also extending a JSON 
                    //    // based payload with the timestamp is a suitable use case.
                    //    context.ApplicationMessage.Payload = Encoding.UTF8.GetBytes(DateTime.Now.ToString("O"));
                    //}

                    //if (context.ApplicationMessage.Topic == "not_allowed_topic")
                    //{
                    //    context.AcceptPublish = false;
                    //    context.CloseConnection = true;
                    //}
                },
                SubscriptionInterceptor = context =>
                {
                    //if (context.TopicFilter.Topic.StartsWith("admin/foo/bar") && context.ClientId != "theAdmin")
                    //{
                    //    context.AcceptSubscription = false;
                    //}

                    //if (context.TopicFilter.Topic.StartsWith("the/secret/stuff") && context.ClientId != "Imperator")
                    //{
                    //    context.AcceptSubscription = false;
                    //    context.CloseConnection = true;
                    //}
                }
            };
            return options;
        }

        private static async void _mqttServer_ApplicationMessageReceived(object sender, MqttApplicationMessageReceivedEventArgs e)
        {

            //Console.WriteLine("Server Received: Should implement code to push data to Kafka");
            //Console.WriteLine("Sender");
            //Console.WriteLine(JsonConvert.SerializeObject(sender));
            //Console.WriteLine("MqttApplicationMessageReceivedEventArgs");

            var sw = Stopwatch.StartNew();
            var msgToKafka = new
            {
                PayLoadStringUTF8 = Encoding.UTF8.GetString(e.ApplicationMessage.Payload),
                ClientMsg = e
            };

            string text = JsonConvert.SerializeObject(msgToKafka);

            //can do with redis queue
            RedisUsage.RedisServices.RedisServices.TryEnqueue(e.ClientId, text);
            Console.WriteLine($"Pushed into Redis MSG: '{text}' to QUEUE: {e.ClientId} in miliseconds: {sw.ElapsedMilliseconds}");

            // or use kafka
            //using (var producer = new Producer<Null, string>(_mqttConfig, null, new StringSerializer(Encoding.UTF8)))
            //{
            //    var result = await producer.ProduceAsync(e.ClientId, null, text);

            //    producer.Flush(100);
            //}
            //sw.Stop();

            //Console.WriteLine($"Pushed into Kafka MSG: '{text}' to TOPIC: {e.ClientId} in miliseconds: {sw.ElapsedMilliseconds}");
        }


        private static void _mqttServer_ClientUnsubscribedTopic(object sender, MqttClientUnsubscribedTopicEventArgs e)
        {
            //throw new NotImplementedException();
        }

        private static void _mqttServer_ClientSubscribedTopic(object sender, MqttClientSubscribedTopicEventArgs e)
        {
            //throw new NotImplementedException();
        }

        private static void _mqttServer_ClientDisconnected(object sender, MqttClientDisconnectedEventArgs e)
        {
            //throw new NotImplementedException();
        }

        private static void _mqttServer_ClientConnected(object sender, MqttClientConnectedEventArgs e)
        {
            //throw new NotImplementedException();
        }

        #region tesing only

        private static void TestPushDataToKafka()
        {
            var topic = "dudu_test";
            var text = "Heello";
            var kafkaHost = ConfigurationManagerExtensions.GetValueByKey("Kafka:Host") ?? "127.0.0.1:9092";

            var config = new Dictionary<string, object>
                                      {
                                        { "bootstrap.servers", kafkaHost },
                                        { "acks", "all" },
                                        { "retries",3 },
                                      };

            Console.WriteLine("Push to Kafka");
            var sw = Stopwatch.StartNew();
            using (var producer = new Producer<string, string>(config, new StringSerializer(Encoding.UTF8), new StringSerializer(Encoding.UTF8)))
            {
                var result = producer.ProduceAsync(topic, null, text).GetAwaiter().GetResult();

                producer.Flush(1);
            }
            sw.Stop();
            Console.WriteLine($"Pushed MSG: '{text}' to TOPIC: {topic} into Kafka in miliseconds: {sw.ElapsedMilliseconds}");
            Console.WriteLine("Press 'Enter' key for close");
        }

        #endregion

    }

    public class RetainedMessageHandler : IMqttServerStorage
    {
        private const string Filename = "C:\\MQTT\\RetainedMessages.json";

        public Task SaveRetainedMessagesAsync(IList<MqttApplicationMessage> messages)
        {
            var directory = Path.GetDirectoryName(Filename);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(Filename, JsonConvert.SerializeObject(messages));
            return Task.FromResult(0);
        }

        public Task<IList<MqttApplicationMessage>> LoadRetainedMessagesAsync()
        {
            IList<MqttApplicationMessage> retainedMessages;
            if (File.Exists(Filename))
            {
                var json = File.ReadAllText(Filename);
                retainedMessages = JsonConvert.DeserializeObject<List<MqttApplicationMessage>>(json);
            }
            else
            {
                retainedMessages = new List<MqttApplicationMessage>();
            }

            return Task.FromResult(retainedMessages);
        }


    }
}
