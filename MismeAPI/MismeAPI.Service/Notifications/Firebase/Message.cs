using FirebaseAdmin.Messaging;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace MismeAPI.Service.Notifications.Firebase
{
    // Summary: Represents a copy of FirebaseAdmin.Messaging.Message fields.
    public class Message
    {
        // Summary: Gets or sets the registration token of the device to which the message should be sent.
        [JsonProperty("token")]
        public string Token { get; set; }

        // Summary: Gets or sets the name of the FCM topic to which the message should be sent.
        // Topic names may contain the /topics/ prefix.
        [JsonIgnore]
        public string Topic { get; set; }

        // Summary: Gets or sets the FCM condition to which the message should be sent. Must be a
        // valid condition string such as "'foo' in topics".
        [JsonProperty("condition")]
        public string Condition { get; set; }

        // Summary: Gets or sets a collection of key-value pairs that will be added to the message
        // as data fields. Keys and the values must not be null.
        [JsonProperty("data")]
        public IReadOnlyDictionary<string, string> Data { get; set; }

        // Summary: Gets or sets the notification information to be included in the message.
        [JsonProperty("notification")]
        public Notification Notification { get; set; }

        // Summary: Gets or sets the Android-specific information to be included in the message.
        [JsonProperty("android")]
        public AndroidConfig Android { get; set; }

        // Summary: Gets or sets the Webpush-specific information to be included in the message.
        [JsonProperty("webpush")]
        public WebpushConfig Webpush { get; set; }

        // Summary: Gets or sets the APNs-specific information to be included in the message.
        [JsonProperty("apns")]
        public ApnsConfig Apns { get; set; }

        // Summary: Gets or sets the FCM options to be included in the message.
        [JsonProperty("fcm_options")]
        public FcmOptions FcmOptions { get; set; }
    }
}
