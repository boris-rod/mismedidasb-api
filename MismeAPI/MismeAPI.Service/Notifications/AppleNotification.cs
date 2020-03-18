using Newtonsoft.Json;

namespace MismeAPI.Service.Notifications
{
    public class AppleNotification
    {
        public class ApsPayload
        {
            [JsonProperty("alert")]
            public string AlertBody { get; set; }
        }

        // Your custom properties as needed

        [JsonProperty("aps")]
        public ApsPayload Aps { get; set; }
    }
}