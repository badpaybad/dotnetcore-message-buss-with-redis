using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RedisUsage.CqrsCore.Mqtt;
using System;
using System.Security.Cryptography;
using System.Text;

namespace Mqtt.DeviceRegisterApis.Controllers
{
    [Route("register")]
    [ApiController]
    public class RegisterController : ControllerBase
    {

        public IActionResult Get(string deviceId)
        {
            //Create a UnicodeEncoder to convert between byte array and string.
            UTF8Encoding ByteConverter = new UTF8Encoding();

            //Create byte arrays to hold original, encrypted, and decrypted data.
            byte[] dataToEncrypt = ByteConverter.GetBytes(deviceId);
            byte[] encryptedData;
            byte[] decryptedData;

            //Create a new instance of RSACryptoServiceProvider to generate
            //public and private key data.
            using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider(1024))
            {


                //Pass the data to ENCRYPT, the public key information 
                //(using RSACryptoServiceProvider.ExportParameters(false),
                //and a boolean flag specifying no OAEP padding.
                var publicKey = RSA.ExportParameters(false);
                var privateKey = RSA.ExportParameters(true);

                var publicKeyComponent = ByteConverter.GetString(publicKey.Modulus);

                var pairKey = new MqttCryptorPair
                {
                    DeviceId = deviceId,
                    RSAParametersPublic = JsonConvert.SerializeObject(publicKey),
                    RSAParametersPrivate = JsonConvert.SerializeObject(privateKey)
                };
                //must store deviceid, publicKey, privateKey to db, this db should be share internal system to other can identity deviceid

                RedisUsage.RedisServices.RedisServices.HashSet("RSA",
                    new System.Collections.Generic.KeyValuePair<string, string>(deviceId + "." + publicKeyComponent
                    , JsonConvert.SerializeObject(pairKey)));

                //encryptedData = RSAEncrypt(dataToEncrypt, publicKey);

                ////Pass the data to DECRYPT, the private key information 
                ////(using RSACryptoServiceProvider.ExportParameters(true),
                ////and a boolean flag specifying no OAEP padding.
                //decryptedData = RSADecrypt(encryptedData, RSA.ExportParameters(true));

                ////Display the decrypted plaintext to the console. 
                //Console.WriteLine("Decrypted plaintext: {0}", ByteConverter.GetString(decryptedData));

                return new JsonResult(publicKeyComponent);
            }


        }

        public static byte[] RSAEncrypt(byte[] DataToEncrypt, RSAParameters RSAKeyInfo, bool DoOAEPPadding = false)
        {

            try
            {
                byte[] encryptedData;
                //Create a new instance of RSACryptoServiceProvider.
                using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider())
                {

                    //Import the RSA Key information. This only needs
                    //toinclude the public key information.
                    RSA.ImportParameters(RSAKeyInfo);

                    //Encrypt the passed byte array and specify OAEP padding.  
                    //OAEP padding is only available on Microsoft Windows XP or
                    //later.  
                    encryptedData = RSA.Encrypt(DataToEncrypt, DoOAEPPadding);
                }
                return encryptedData;
            }
            //Catch and display a CryptographicException  
            //to the console.
            catch (CryptographicException e)
            {
                Console.WriteLine(e.Message);

                return null;
            }

        }

        public static byte[] RSADecrypt(byte[] DataToDecrypt, RSAParameters RSAKeyInfo, bool DoOAEPPadding = false)
        {
            try
            {
                byte[] decryptedData;
                //Create a new instance of RSACryptoServiceProvider.
                using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider())
                {
                    //Import the RSA Key information. This needs
                    //to include the private key information.
                    RSA.ImportParameters(RSAKeyInfo);

                    //Decrypt the passed byte array and specify OAEP padding.  
                    //OAEP padding is only available on Microsoft Windows XP or
                    //later.  
                    decryptedData = RSA.Decrypt(DataToDecrypt, DoOAEPPadding);
                }
                return decryptedData;
            }
            //Catch and display a CryptographicException  
            //to the console.
            catch (CryptographicException e)
            {
                Console.WriteLine(e.ToString());

                return null;
            }

        }
    }
}
