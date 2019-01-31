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

namespace Mqtt.MqttServerBrocker
{
    class Program
    {
        static MqttFactory _mqttfactory = new MqttFactory();
        static IMqttServer _mqttServer;

        public static void Main(string[] args)
        {
            _mqttServer = _mqttfactory.CreateMqttServer();
            _mqttServer.ClientConnected += _mqttServer_ClientConnected;
            _mqttServer.ClientDisconnected += _mqttServer_ClientDisconnected;
            _mqttServer.ClientSubscribedTopic += _mqttServer_ClientSubscribedTopic;
            _mqttServer.ClientUnsubscribedTopic += _mqttServer_ClientUnsubscribedTopic;
            _mqttServer.ApplicationMessageReceived += _mqttServer_ApplicationMessageReceived;

            MqttServerOptions options = BuildMqttServerOptions();

            var tserver = _mqttServer.StartAsync(options);
            tserver.Wait();

            Console.WriteLine("server started");

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

                //if (cmd.StartsWith("c-p"))
                //{
                //    //if (_mqttClient == null)
                //    //{
                //    //    tclient = InitClient();
                //    //    tclient.Wait();
                //    //}

                //    Console.WriteLine("Client push message");

                //    var applicationMessage = new MqttApplicationMessageBuilder()
                //      .WithTopic("A/B/C")
                //      .WithPayload("Hello World to server")
                //      .WithAtLeastOnceQoS()
                //      .Build();

                //    //var tclientPush = _mqttClient.PublishAsync(applicationMessage);
                //    //tclientPush.Wait();
                //}

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

        private static void _mqttServer_ApplicationMessageReceived(object sender, MqttApplicationMessageReceivedEventArgs e)
        {
            
            Console.WriteLine("Server Received: Should implement code to push data to Kafka");
            Console.WriteLine("Sender");
            Console.WriteLine(JsonConvert.SerializeObject(sender));
            Console.WriteLine("MqttApplicationMessageReceivedEventArgs");
            Console.WriteLine(JsonConvert.SerializeObject(e));

            string text = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);

            var config = new Dictionary<string, object>
                                      {
                                        { "bootstrap.servers", "127.0.0.1:2181" },
                                        { "acks", "all" }
                                      };

            Console.WriteLine("Push to Kafka");
            var sw = Stopwatch.StartNew();
            using (var producer = new Producer<Null, string>(config, null, new StringSerializer(Encoding.UTF8)))
            {
                var result = producer.ProduceAsync(e.ClientId, null, text).GetAwaiter().GetResult();

                producer.Flush(1000);
            }
            sw.Stop();
            Console.WriteLine($"Pushed MSG: {text} to TOPIC: {e.ClientId} into Kafka in miliseconds: {sw.ElapsedMilliseconds}");

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
