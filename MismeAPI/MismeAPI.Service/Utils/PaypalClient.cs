using System;
using PayPalCheckoutSdk.Core;
using PayPalHttp;
using System.IO;
using System.Text;
using System.Runtime.Serialization.Json;
using Microsoft.Extensions.Configuration;

namespace MismeAPI.Service.Utils
{
    public class PayPalClient
    {
        /**
            Setting up PayPal environment with credentials with sandbox cerdentails.
            For Live, this should be LiveEnvironment Instance.
         */

        public static PayPalEnvironment Environment(IConfiguration configuration)
        {
            return new SandboxEnvironment(configuration.GetSection("Paypal")["SandboxClientID"], configuration.GetSection("Paypal")["SandboxClientSecret"]);
        }

        /**
            Returns PayPalHttpClient instance which can be used to invoke PayPal API's.
         */

        public static HttpClient Client(IConfiguration configuration)
        {
            return new PayPalHttpClient(Environment(configuration));
        }

        public static HttpClient Client(IConfiguration configuration, string refreshToken)
        {
            return new PayPalHttpClient(Environment(configuration), refreshToken);
        }

        /**
            This method can be used to Serialize Object to JSON string.
        */

        public static String ObjectToJSONString(Object serializableObject)
        {
            MemoryStream memoryStream = new MemoryStream();
            var writer = JsonReaderWriterFactory.CreateJsonWriter(
                        memoryStream, Encoding.UTF8, true, true, "  ");
            DataContractJsonSerializer ser = new DataContractJsonSerializer(serializableObject.GetType(), new DataContractJsonSerializerSettings { UseSimpleDictionaryFormat = true });
            ser.WriteObject(writer, serializableObject);
            memoryStream.Position = 0;
            StreamReader sr = new StreamReader(memoryStream);
            return sr.ReadToEnd();
        }
    }
}
