using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RedisUsage.CqrsCore.Extensions;
using RedisUsage.CqrsCore.Mqtt;
using System;
using System.Security.Cryptography;

namespace Mqtt.DeviceRegisterApis.Controllers
{
    [Route("register")]
    [ApiController]
    public class RegisterController : ControllerBase
    {

        public IActionResult Get(string deviceId)
        {
            RSAParameters publicKey;
            RSAParameters privateKey;

            StringCipher.RsaGenerate(out publicKey, out privateKey);

            var topicForClient = StringCipher.GetMd5Hash(Convert.ToBase64String(publicKey.Modulus));

            var pairKey = new MqttCryptorPair
            {
                TopicForClient = topicForClient,
                DeviceId = deviceId,
                RSAParametersPublic = JsonConvert.SerializeObject(publicKey),
                RSAParametersPrivate = JsonConvert.SerializeObject(privateKey)
            };

            //must store deviceid, publicKey, privateKey to db, 
            //this db should be share internal system to other can identity deviceid

            var redisForPublicKey = deviceId + "/" + StringCipher.GetMd5Hash(topicForClient);

            string secret = JsonConvert.SerializeObject(pairKey);

            RedisUsage.RedisServices.RedisServices.HashSet("RSA",
                new System.Collections.Generic.KeyValuePair<string, string>(redisForPublicKey
                , secret));

            return base.Content(secret);

        }

    }
}
