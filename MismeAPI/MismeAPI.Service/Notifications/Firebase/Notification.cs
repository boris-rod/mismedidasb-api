using Newtonsoft.Json;

namespace MismeAPI.Service.Notifications.Firebase
{
    /// <summary>
    /// Represents a copy of FirebaseAdmin.Messaging.Notification
    /// </summary>
    public class Notification
    {
        // Summary: Gets or sets the title of the notification.
        [JsonProperty("title")]
        public string Title { get; set; }

        // Summary: Gets or sets the body of the notification.
        [JsonProperty("body")]
        public string Body { get; set; }

        // Summary: Gets or sets the URL of the image to be displayed in the notification.
        [JsonProperty("image")]
        public string ImageUrl { get; set; }

        // Summary: Gets or sets the sound in the notification.
        [JsonProperty("sound")]
        public string Sound { get; set; }
    }
}
