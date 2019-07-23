using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Sales.Messages;
using Shouldly;
using Xunit;

namespace ContractTester.Tests.SalesBC
{
    public class PlaceOrderTests
    {
        public static HttpClient Client = new HttpClient();
        public const string ValidPlaceOrderContractId = "DFBF39EA-62A1-4C51-78F7-08D70F8D1BDD";

        [Fact]
        public async Task ShouldReturnSuccessForValidContract()
        {
            var placeOrder = new PlaceOrder
            {
                OrderId = "123abc",
                ItemName = "Frying Pan",
                OrderDate = DateTime.Now
            };

            var placeOrderMessage = new PlaceOrderContract
            {
                ContractId = Guid.Parse(ValidPlaceOrderContractId),
                Message = new PlaceOrderMessage
                {
                    Id = Guid.NewGuid(),
                    ItemName = placeOrder.ItemName,
                    OrderDate = placeOrder.OrderDate,
                    OrderId = placeOrder.OrderId,
                    Timestamp = DateTime.Now
                }
            };

            var stringContent = new StringContent(JsonConvert.SerializeObject(placeOrderMessage), Encoding.UTF8, "application/json");

            var response = await Client.PostAsync("https://localhost:5001/api/TestMessage", stringContent);

            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task ShouldReturnBadRequestForInvalidMessageIdForContract()
        {
            var placeOrder = new PlaceOrder
            {
                OrderId = "456def",
                ItemName = "Griddle",
                OrderDate = DateTime.Now
            };

            var placeOrderMessage = new PlaceOrderContract
            {
                ContractId = Guid.Parse(ValidPlaceOrderContractId),
                Message = new PlaceOrderMessage
                {
                    Id = null,
                    ItemName = placeOrder.ItemName,
                    OrderDate = placeOrder.OrderDate,
                    OrderId = placeOrder.OrderId,
                    Timestamp = DateTime.Now
                }
            };

            var stringContent = new StringContent(JsonConvert.SerializeObject(placeOrderMessage), Encoding.UTF8, "application/json");

            var response = await Client.PostAsync("https://localhost:5001/api/TestMessage", stringContent);

            response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
            var responseString = await response.Content.ReadAsStringAsync();
            responseString.ShouldContain("Message does not match contract");
        }

        [Fact]
        public async Task ShouldReturnBadRequestForInvalidContractIdForContract()
        {
            var placeOrder = new PlaceOrder
            {
                OrderId = "678ghi",
                ItemName = "Nu wave oven",
                OrderDate = DateTime.Now
            };

            var invalidGuid = Guid.NewGuid();

            var placeOrderMessage = new PlaceOrderContract
            {
                ContractId = invalidGuid,
                Message = new PlaceOrderMessage
                {
                    Id = null,
                    ItemName = placeOrder.ItemName,
                    OrderDate = placeOrder.OrderDate,
                    OrderId = placeOrder.OrderId,
                    Timestamp = DateTime.Now
                }
            };

            var stringContent = new StringContent(JsonConvert.SerializeObject(placeOrderMessage), Encoding.UTF8, "application/json");

            var response = await Client.PostAsync("https://localhost:5001/api/TestMessage", stringContent);

            response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
            var responseString = await response.Content.ReadAsStringAsync();
            responseString.ShouldContain("Unable to test message; confirm that your request message is valid JSON.");
        }
    }

    public class PlaceOrderContract
    {
        public Guid ContractId { get; set; }
        public PlaceOrderMessage Message { get; set; }
    }

    public class PlaceOrderMessage
    {
        public Guid? Id { get; set; }
        public string ItemName { get; set; }
        public DateTime OrderDate { get; set; }
        public string OrderId { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
