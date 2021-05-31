using Newtonsoft.Json;

namespace MismeAPI.Common.DTO
{
    public class CreateCheckoutSessionRequest
    {
        /// <summary>
        /// Stripe Price ID - Get from API endpoint to get group-service prices
        /// </summary>
        [JsonProperty("priceId")]
        public string PriceId { get; set; }

        public string SuccessUrl { get; set; }
        public string CancelUrl { get; set; }
    }
}
