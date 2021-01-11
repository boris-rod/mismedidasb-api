using Microsoft.Extensions.Configuration;
using MismeAPI.Common.Exceptions;
using MismeAPI.Data.Entities.NonDatabase;
using MismeAPI.Service.Utils;
using PayPalCheckoutSdk.Orders;
using PayPalCheckoutSdk.Payments;
using PayPalHttp;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MismeAPI.Service.Impls
{
    public class AppleAppStoreService : IAppleAppStoreService
    {
        private readonly IConfiguration _configuration;

        public AppleAppStoreService(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public bool IsReceiptValid(AppleReceipt receipt)
        {
            return receipt != null && receipt.Status == 0;
        }

        public AppleReceipt GetReceipt(string receiptData)
        {
            var url = _configuration["InAppPurshase:VerifyReceiptUrl"];
            AppleReceipt result = null;

            string post = PostRequest(url, ConvertReceiptToPost(receiptData));

            if (!string.IsNullOrEmpty(post))
            {
                try { result = new AppleReceipt(post); }
                catch (InvalidDataException e) { throw e; }
                catch (Exception) { result = null; }
            }

            return result;
        }

        #region Private Static Methods

        /// <summary>
        /// Make a string with the receipt encoded
        /// </summary>
        /// <param name="receipt"></param>
        /// <returns></returns>
        private static string ConvertReceiptToPost(string receipt)
        {
            //string itunesDecodedReceipt = Encoding.UTF8.GetString(ReceiptVerification.ConvertAppStoreTokenToBytes(receipt.Replace("<", string.Empty).Replace(">", string.Empty))).Trim();
            string itunesDecodedReceipt = receipt.Replace("<", string.Empty).Replace(">", string.Empty).Trim();
            string encodedReceipt = Base64Encode(itunesDecodedReceipt);
            //return string.Format(@"{{""receipt-data"":""{0}"", ""password"":""{1}""}}", encodedReceipt, "c6299613b6f849faada7ddf9c170b756");
            return string.Format(@"{{""receipt-data"":""{0}""}}", encodedReceipt);
        }

        /// <summary>
        /// Base64 Encoding
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private static string Base64Encode(string str)
        {
            byte[] encbuff = System.Text.Encoding.UTF8.GetBytes(str);
            return Convert.ToBase64String(encbuff);
        }

        /// <summary>
        /// Base64 Decoding
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private static string Base64Decode(string str)
        {
            byte[] decbuff = Convert.FromBase64String(str);
            return System.Text.Encoding.UTF8.GetString(decbuff);
        }

        /// <summary>
        /// Sends a request to the server and reads the response
        /// </summary>
        /// <param name="url"></param>
        /// <param name="postData"></param>
        /// <returns></returns>
        private static string PostRequest(string url, string postData)
        {
            byte[] byteArray = Encoding.UTF8.GetBytes(postData);
            return PostRequest(url, byteArray);
        }

        /// <summary>
        /// Sends a request to the server and reads the response
        /// </summary>
        /// <param name="url"></param>
        /// <param name="byteArray"></param>
        /// <returns></returns>
        private static string PostRequest(string url, byte[] byteArray)
        {
            try
            {
                WebRequest request = HttpWebRequest.Create(url);
                request.Method = "POST";
                request.ContentLength = byteArray.Length;
                request.ContentType = "text/plain";

                using (System.IO.Stream dataStream = request.GetRequestStream())
                {
                    dataStream.Write(byteArray, 0, byteArray.Length);
                    dataStream.Close();
                }

                using (WebResponse r = request.GetResponse())
                {
                    using (System.IO.StreamReader sr = new System.IO.StreamReader(r.GetResponseStream()))
                    {
                        return sr.ReadToEnd();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw ex;
            }
        }

        #endregion Private Static Methods
    }
}
