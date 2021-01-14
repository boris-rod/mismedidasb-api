using MismeAPI.Common.Exceptions;
using Newtonsoft.Json.Linq;
using Serilog;
using System;
using System.Collections.Generic;

namespace MismeAPI.Data.Entities.NonDatabase
{
    [Serializable()]
    public class AppleReceipt
    {
        public AppleReceipt()
        {
        }

        #region Constructor

        /// <summary>
        /// Creates the receipt from Apple's Response
        /// </summary>
        /// <param name="receipt"></param>
        public AppleReceipt(string receipt)
        {
            var receipts = new List<AppleReceipt>();

            JObject json = JObject.Parse(receipt);

            int status = -1;

            int.TryParse(json["status"].ToString(), out status);
            Status = status;

            var errorMsg = "";
            if (Status == 21000)
            {
                errorMsg = "IAP Error 21000 - The request to the App Store was not made using the HTTP POST request method.";
                throw new Exception(errorMsg);
            }

            if (Status == 21001)
            {
                errorMsg = "IAP Error 21001 - This status code is no longer sent by the App Store.";
                throw new Exception(errorMsg);
            }

            if (Status == 21002)
            {
                errorMsg = "IAP Error 21002 - The data in the receipt-data property was malformed or the service experienced a temporary issue. Try again.";
                throw new Exception(errorMsg);
            }

            if (Status == 21003)
            {
                errorMsg = "IAP Error 21003 - The receipt could not be authenticated.";
                throw new Exception(errorMsg);
            }

            if (Status == 21004)
            {
                errorMsg = "IAP Error 21004 - The shared secret you provided does not match the shared secret on file for your account.";
                throw new Exception(errorMsg);
            }

            if (Status == 21005)
            {
                errorMsg = "IAP Error 21005 - The receipt server was temporarily unable to provide the receipt. Try again.";
                throw new Exception(errorMsg);
            }

            if (Status == 21006)
            {
                errorMsg = "IAP Error 21006 - This receipt is valid but the subscription has expired. When this status code is returned to your server, the receipt data is also decoded and returned as part of the response. Only returned for iOS 6-style transaction receipts for auto-renewable subscriptions.";
                throw new Exception(errorMsg);
            }

            if (Status == 21007)
            {
                throw new BadGateway("This receipt is from the test environment, but it was sent to the production environment for verification.");
            }

            if (Status == 21008)
                throw new BadGateway("This receipt is from the production environment, but it was sent to the test environment for verification.");

            if (Status == 21009)
            {
                errorMsg = "IAP Error 21009 - Internal data access error. Try again later.";
                throw new Exception(errorMsg);
            }

            if (Status == 21010)
            {
                errorMsg = "IAP Error 21010 - The user account cannot be found or has been deleted.";
                throw new Exception(errorMsg);
            }

            Console.WriteLine("#ReceiptValidation returns status: " + Status.ToString());

            // Receipt is actually a child
            json = (JObject)json["receipt"];

            var inApp = (JArray)json["in_app"];

            foreach (var item in inApp)
            {
                this.OriginalTransactionId = item["original_transaction_id"].ToString();
                this.ProductId = item["product_id"].ToString();

                DateTime purchaseDate = DateTime.MinValue;
                if (DateTime.TryParseExact(item["purchase_date"].ToString().Replace(" Etc/GMT", string.Empty).Replace("\"", string.Empty).Trim(), "yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out purchaseDate))
                    this.PurchaseDate = purchaseDate;

                DateTime originalPurchaseDate = DateTime.MinValue;
                if (DateTime.TryParseExact(item["original_purchase_date"].ToString().Replace(" Etc/GMT", string.Empty).Replace("\"", string.Empty).Trim(), "yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out originalPurchaseDate))
                    this.OriginalPurchaseDate = originalPurchaseDate;

                int quantity = 1;
                int.TryParse(item["quantity"].ToString(), out quantity);
                this.Quantity = quantity;

                this.BundleIdentifier = json["bundle_id"].ToString();

                this.TransactionId = item["transaction_id"].ToString();

                var receiptObject = new AppleReceipt();
                receiptObject.OriginalTransactionId = OriginalTransactionId;
                receiptObject.ProductId = ProductId;
                receiptObject.PurchaseDate = PurchaseDate;
                receiptObject.Quantity = Quantity;
                receiptObject.BundleIdentifier = BundleIdentifier;
                receiptObject.OriginalPurchaseDate = OriginalPurchaseDate;
                receiptObject.TransactionId = TransactionId;
                receiptObject.Status = Status;

                receipts.Add(receiptObject);
            }

            Receipts = receipts;
        }

        #endregion Constructor

        #region Properties

        public string OriginalTransactionId { get; set; }

        public string ProductId { get; set; }

        public DateTime? PurchaseDate { get; set; }

        public int Quantity { get; set; }

        public string BundleIdentifier { get; set; }

        public DateTime? OriginalPurchaseDate { get; set; }

        public string TransactionId { get; set; }

        public int Status { get; set; }

        public IEnumerable<AppleReceipt> Receipts { get; set; }

        #endregion Properties
    }
}
