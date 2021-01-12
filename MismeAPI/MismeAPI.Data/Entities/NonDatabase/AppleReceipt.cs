using MismeAPI.Common.Exceptions;
using Newtonsoft.Json.Linq;
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

            if (Status == 21002)
                throw new InvalidDataException("The data in the receipt-data property was malformed or the service experienced a temporary issue. Try again.");

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
