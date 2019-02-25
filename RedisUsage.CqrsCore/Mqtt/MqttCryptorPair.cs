using System;
using System.Collections.Generic;
using System.Text;

namespace RedisUsage.CqrsCore.Mqtt
{
   public class MqttCryptorPair
    {
        public string DeviceId { get; set; }
        public string RSAParametersPublic { get; set; }
        public string RSAParametersPrivate { get; set; }
    }
}
