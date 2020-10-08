using Microsoft.Extensions.Configuration;
using MismeAPI.Service.Utils;
using PayPalCheckoutSdk.Orders;
using PayPalCheckoutSdk.Payments;
using PayPalHttp;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MismeAPI.Service.Impls
{
    public class PaypalService : IPaypalService
    {
        private readonly IConfiguration _configuration;

        public PaypalService(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        //2. Set up your server to receive a call from the client
        /*
          Method to create order

          @param debug true = print response data
          @return HttpResponse<Order> response received from API
          @throws IOException Exceptions from API if any
        */
        public async Task<HttpResponse> CreateOrder(bool debug = false)
        {
            var request = new OrdersCreateRequest();
            request.Prefer("return=representation");
            request.RequestBody(BuildRequestBody());
            //3. Call PayPal to set up a transaction
            var response = await PayPalClient.Client(_configuration).Execute(request);

            if (debug)
            {
                var result = response.Result<Order>();
                Console.WriteLine("Status: {0}", result.Status);
                Console.WriteLine("Order Id: {0}", result.Id);
                Console.WriteLine("Intent: {0}", result.CheckoutPaymentIntent);
                Console.WriteLine("Links:");
                foreach (PayPalCheckoutSdk.Orders.LinkDescription link in result.Links)
                {
                    Console.WriteLine("\t{0}: {1}\tCall Type: {2}", link.Rel, link.Href, link.Method);
                }
                AmountWithBreakdown amount = result.PurchaseUnits[0].AmountWithBreakdown;
                Console.WriteLine("Total Amount: {0} {1}", amount.CurrencyCode, amount.Value);
            }

            return response;
        }

        public async Task<HttpResponse> CaptureOrder(string OrderId, bool debug = false)
        {
            var request = new OrdersCaptureRequest(OrderId);
            request.Prefer("return=representation");
            request.RequestBody(new OrderActionRequest());
            //3. Call PayPal to capture an order
            var response = await PayPalClient.Client(_configuration).Execute(request);
            //4. Save the capture ID to your database. Implement logic to save capture to your database for future reference.

            if (debug)
            {
                var result = response.Result<Order>();
                Console.WriteLine("Status: {0}", result.Status);
                Console.WriteLine("Order Id: {0}", result.Id);
                Console.WriteLine("Intent: {0}", result.CheckoutPaymentIntent);
                Console.WriteLine("Links:");
                foreach (PayPalCheckoutSdk.Orders.LinkDescription link in result.Links)
                {
                    Console.WriteLine("\t{0}: {1}\tCall Type: {2}", link.Rel, link.Href, link.Method);
                }
                Console.WriteLine("Capture Ids: ");
                foreach (PurchaseUnit purchaseUnit in result.PurchaseUnits)
                {
                    foreach (PayPalCheckoutSdk.Orders.Capture capture in purchaseUnit.Payments.Captures)
                    {
                        Console.WriteLine("\t {0}", capture.Id);
                    }
                }
                AmountWithBreakdown amount = result.PurchaseUnits[0].AmountWithBreakdown;
                Console.WriteLine("Buyer:");
                Console.WriteLine("\tEmail Address: {0}\n\tName: {1}\n\tPhone Number: {2}{3}", result.Payer.Email, result.Payer.Name.FullName, result.Payer.PhoneWithType.PhoneNumber.CountryCallingCode, result.Payer.PhoneWithType.PhoneNumber.NationalNumber);
            }

            return response;
        }

        public async Task<HttpResponse> AuthorizeOrder(string OrderId, bool debug = false)
        {
            var request = new OrdersAuthorizeRequest(OrderId);
            request.Prefer("return=representation");
            request.RequestBody(new AuthorizeRequest());
            //3. Call PayPal to authorization an order
            var response = await PayPalClient.Client(_configuration).Execute(request);
            //4. Save the authorization ID to your database. Implement logic to save the authorization to your database for future reference.
            if (debug)
            {
                var result = response.Result<Order>();
                Console.WriteLine("Status: {0}", result.Status);
                Console.WriteLine("Order Id: {0}", result.Id);
                Console.WriteLine("Authorization Id: {0}",
                                result.PurchaseUnits[0].Payments.Authorizations[0].Id);
                Console.WriteLine("Intent: {0}", result.CheckoutPaymentIntent);
                Console.WriteLine("Links:");
                foreach (PayPalCheckoutSdk.Orders.LinkDescription link in result.Links)
                {
                    Console.WriteLine("\t{0}: {1}\tCall Type: {2}", link.Rel,
                                                                    link.Href,
                                                                    link.Method);
                }
                AmountWithBreakdown amount = result.PurchaseUnits[0].AmountWithBreakdown;
                Console.WriteLine("Buyer:");
                Console.WriteLine("\tEmail Address: {0}", result.Payer.Email);
                Console.WriteLine("Response JSON: \n {0}", PayPalClient.ObjectToJSONString(result));
            }

            return response;
        }

        public async Task<HttpResponse> CaptureAuth(string authorizationId, bool debug = false)
        {
            var request = new AuthorizationsCaptureRequest(authorizationId);
            request.Prefer("return=representation");
            request.RequestBody(new CaptureRequest());
            var response = await PayPalClient.Client(_configuration).Execute(request);

            if (debug)
            {
                var result = response.Result<PayPalCheckoutSdk.Orders.Capture>();
                Console.WriteLine("Status: {0}", result.Status);
                Console.WriteLine("Order Id: {0}", result.Id);
                Console.WriteLine("Links:");
                foreach (PayPalCheckoutSdk.Orders.LinkDescription link in result.Links)
                {
                    Console.WriteLine("\t{0}: {1}\tCall Type: {2}", link.Rel, link.Href, link.Method);
                }
                Console.WriteLine("Response JSON: \n {0}", PayPalClient.ObjectToJSONString(result));
            }

            return response;
        }

        /*
          Method to generate sample create order body with CAPTURE intent

          @return OrderRequest with created order request
         */
        private static OrderRequest BuildRequestBody()
        {
            OrderRequest orderRequest = new OrderRequest()
            {
                CheckoutPaymentIntent = "CAPTURE",

                ApplicationContext = new ApplicationContext
                {
                    BrandName = "EXAMPLE INC",
                    LandingPage = "BILLING",
                    UserAction = "CONTINUE",
                    ShippingPreference = "SET_PROVIDED_ADDRESS"
                },
                PurchaseUnits = new List<PurchaseUnitRequest>
                {
                    new PurchaseUnitRequest{
                        ReferenceId =  "PUHF",
                        Description = "Sporting Goods",
                        CustomId = "CUST-HighFashions",
                        SoftDescriptor = "HighFashions",
                        AmountWithBreakdown = new AmountWithBreakdown
                        {
                          CurrencyCode = "EUR",
                          Value = "180.00"
                        },
                        Items = new List<Item>
                        {
                          new Item
                          {
                            Name = "T-shirt",
                            Description = "Green XL",
                            Sku = "sku01",
                            UnitAmount = new PayPalCheckoutSdk.Orders.Money
                            {
                              CurrencyCode = "USD",
                              Value = "90.00"
                            },
                            Quantity = "1",
                            Category = "PHYSICAL_GOODS"
                          }
                        }
                      }
                    }
            };

            return orderRequest;
        }
    }
}
