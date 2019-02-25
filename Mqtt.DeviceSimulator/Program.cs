using MQTTnet;
using MQTTnet.Client;
using Newtonsoft.Json;
using RedisUsage.CqrsCore.Ef;
using RedisUsage.CqrsCore.Extensions;
using RedisUsage.CqrsCore.Mqtt;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Mqtt.DeviceSimulator
{
    class Program
    {
        static string _secret;
        static MqttCryptorPair _parseSecret;
        const string _deviceId = "dudu";

        static MqttFactory _mqttfactory = new MqttFactory();
        static MQTTnet.Client.IMqttClient _mqttClient;

        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            TryToRegisterAndGetPublicKey();

            MqttClientInitAndConnect().GetAwaiter().GetResult();

            while (true)
            {
                var cmd = Console.ReadLine();
                if (cmd == "q") { Environment.Exit(0); return; }

                if (cmd == "t")
                {
                    SendTestMsg().GetAwaiter().GetResult();
                }
            }
        }



        private static void TryToRegisterAndGetPublicKey()
        {
            if (!string.IsNullOrEmpty(_secret)) return;

            using (var http = new HttpClient())
            {
                http.BaseAddress = new Uri("https://localhost:44329/register?deviceid=" + _deviceId);

                _secret = http.GetStringAsync(http.BaseAddress).Result;
            }

            _parseSecret = JsonConvert.DeserializeObject<MqttCryptorPair>(_secret);   
        }

        private static async Task MqttClientInitAndConnect()
        {
            if (_mqttClient == null)
            {
                _mqttClient = _mqttfactory.CreateMqttClient();

                var mqttHost = ConfigurationManagerExtensions.GetValueByKey("Mqtt:Host") ?? "127.0.0.1";

                //VietNguyen MQTT server
                //mqttHost = "mqtt.edsolabs.com:1883";
                //mqttHost = "lb-mqtt-broker-61454191.ap-southeast-1.elb.amazonaws.com";

                var uid = _deviceId;
                var pwd = StringCipher.GetMd5Hash(_parseSecret.TopicForClient);

                var option = new MQTTnet.Client.MqttClientOptions
                {
                    ChannelOptions = new MQTTnet.Client.MqttClientTcpOptions
                    {
                        Server = mqttHost,
                    },
                    ClientId = _deviceId + "/" + pwd,
                    Credentials = new MqttClientCredentials()
                    {
                        Username = uid,
                        Password = pwd
                    }

                    //KeepAlivePeriod = new TimeSpan(0, 0, 1)
                };

                _mqttClient.ApplicationMessageReceived += (sender, e) =>
                {
                    Console.WriteLine("Client Received: Do anything you want");
                    Console.WriteLine("Sender");
                    Console.WriteLine(JsonConvert.SerializeObject(sender));
                    Console.WriteLine("MqttApplicationMessageReceivedEventArgs");
                    Console.WriteLine(JsonConvert.SerializeObject(e));
                };

                _mqttClient.Disconnected += (sender, e) =>
                {
                    //try reconnect
                    _mqttClient.ConnectAsync(option);
                };
              
                var connectedReponse = await _mqttClient.ConnectAsync(option);

                await _mqttClient.SubscribeAsync(new List<TopicFilter>() {
                    new TopicFilterBuilder()
                    .WithTopic(_mqttClient.Options.ClientId)
                    .WithExactlyOnceQoS()
                    .Build()

                });

            }
        }

        private static async Task SendTestMsg()
        {
            string topic ="test/"+ _deviceId+ "/"+ _parseSecret.TopicForClient;

            //VietNguyen MQTT server
            //topic = "emissions";
          var  msg = JsonConvert.SerializeObject(new 
            {
                device_id = _deviceId,
                emissions = 12.234,
                velocity = new  { x = 1, y = 2 },
                location = new 
                {
                    lat = 3,
                    lon = 4
                }
            });

            var publicKey = JsonConvert.DeserializeObject<RSAParameters>(_parseSecret.RSAParametersPublic);

            var hashMsg = StringCipher.GetMd5Hash(msg);
            var encryptedMsg = StringCipher.RsaEncrypt(msg, publicKey);

            var payload = hashMsg + "." + encryptedMsg;

            var applicationMessage = new MqttApplicationMessageBuilder()
              .WithTopic(topic)
              .WithPayload(payload)
              .WithExactlyOnceQoS()//QoS=3
              .Build();

            await _mqttClient.PublishAsync(applicationMessage);
        }
    }
}
