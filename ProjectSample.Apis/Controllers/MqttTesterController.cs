using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MQTTnet;
using MQTTnet.Client;
using Newtonsoft.Json;
using RedisUsage.CqrsCore.Ef;

namespace ProjectSample.Apis.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MqttTesterController : ControllerBase
    {
        MqttFactory _mqttfactory = new MqttFactory();
        MQTTnet.Client.IMqttClient _mqttClient;

        [HttpPost]
        [Route("ClientSendData")]
        public async Task<object> ClientSendData(string msg)
        {
            await InitClient();

            string topic = _mqttClient.Options.ClientId;

            //VietNguyen MQTT server
            //topic = "emissions";
            msg = JsonConvert.SerializeObject(new TestForVietNguyen()
            {
                device_id = 211284,
                emissions = 12.234,
                velocity = new TestForVietNguyen.Velocity { x = 1, y = 2 },
                location = new TestForVietNguyen.Location
                {
                    lat = 3,
                    lon = 4
                }
            });

            var applicationMessage = new MqttApplicationMessageBuilder()
              .WithTopic(topic)
              .WithPayload(msg)
              .WithAtLeastOnceQoS()
              .Build();

            await _mqttClient.PublishAsync(applicationMessage);

            return new
            {
                Success = true,
                Topic = topic
            };
        }

        private async Task InitClient()
        {
            if (_mqttClient == null)
            {
                _mqttClient = _mqttfactory.CreateMqttClient();
                
                var mqttHost = ConfigurationManagerExtensions.GetValueByKey("Mqtt:Host") ?? "127.0.0.1";

                //VietNguyen MQTT server
                //mqttHost = "mqtt.edsolabs.com:1883";
                //mqttHost = "lb-mqtt-broker-61454191.ap-southeast-1.elb.amazonaws.com";
            
                var uid = "xxx";
                var pwd = "xxx";

                var option = new MQTTnet.Client.MqttClientOptions
                {
                    ChannelOptions = new MQTTnet.Client.MqttClientTcpOptions
                    {
                        Server = mqttHost,
                        Port = 8888
                    },
                    ClientId = "dudu_" + Guid.NewGuid().ToString(),
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
                    .WithAtLeastOnceQoS()
                    .Build()

                });

            }
        }

    }

    public class TestForVietNguyen
    {
        public class Velocity
        {
            public int x { get; set; }
            public int y { get; set; }
        }
        public class Location
        {
            public double lat { get; set; }
            public double lon { get; set; }
        }

        public Velocity velocity { get; set; }
        public double emissions { get; set; }
        public Location location { get; set; }
        public int device_id { get; set; }
    }
}
