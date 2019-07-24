using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Sales.Events;
using Shouldly;
using Xunit;

namespace ContractTester.Tests.SalesBC
{
    using static Testing;

    public class OrderCompletedTests
    {
        public static HttpClient Client = new HttpClient();
        private readonly ContractTesterSettings _contractTesterSettings;

        public OrderCompletedTests()
        {
            _contractTesterSettings = GetContractTesterApplicationConfigurationSettings();
        }

        [Fact]
        public async Task ShouldReturnSuccessForValidContract()
        {
            var orderCompleted = new OrderCompleted {OrderId = "order 111"};

            var placeOrderMessage = new OrderCompletedContract
            {
                ContractId = Guid.Parse(_contractTesterSettings.ValidOrderCompletedContractId),
                Message = new OrderCompleteMessageDetails
                {
                    Id = Guid.NewGuid(),
                    OrderId = orderCompleted.OrderId,
                    Timestamp = DateTime.Now
                }
            };

            var stringContent = new StringContent(JsonConvert.SerializeObject(placeOrderMessage), Encoding.UTF8, "application/json");
            var response = await Client.PostAsync(_contractTesterSettings.TestMessageApiUrl, stringContent);

            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task ShouldReturnBadRequestForInvalidMessageIdForContract()
        {
            var orderCompleted = new OrderCompleted { OrderId = "order 222" };

            var placeOrderMessage = new OrderCompletedContract
            {
                ContractId = Guid.Parse(_contractTesterSettings.ValidOrderCompletedContractId),
                Message = new OrderCompleteMessageDetails
                {
                    Id = null,
                    OrderId = orderCompleted.OrderId,
                    Timestamp = DateTime.Now
                }
            };

            var stringContent = new StringContent(JsonConvert.SerializeObject(placeOrderMessage), Encoding.UTF8, "application/json");
            var response = await Client.PostAsync(_contractTesterSettings.TestMessageApiUrl, stringContent);
            response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

            var responseString = await response.Content.ReadAsStringAsync();
            responseString.ShouldContain("Message does not match contract");
        }

        [Fact]
        public async Task ShouldReturnBadRequestForInvalidContractIdForContract()
        {
            var invalidGuid = Guid.NewGuid();
            var orderCompleted = new OrderCompleted { OrderId = "order 333" };

            var placeOrderMessage = new OrderCompletedContract
            {
                ContractId = invalidGuid,
                Message = new OrderCompleteMessageDetails
                {
                    Id = Guid.NewGuid(),
                    OrderId = orderCompleted.OrderId,
                    Timestamp = DateTime.Now
                }
            };

            var stringContent = new StringContent(JsonConvert.SerializeObject(placeOrderMessage), Encoding.UTF8, "application/json");
            var response = await Client.PostAsync(_contractTesterSettings.TestMessageApiUrl, stringContent);
            response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

            var responseString = await response.Content.ReadAsStringAsync();
            responseString.ShouldContain("Unable to test message; confirm that your request message is valid JSON.");
        }

        [Fact]
        public async Task ShouldReturnBadRequestForInvalidPropertyForContract()
        {
            var placeOrderMessage = new OrderCompletedContract
            {
                ContractId = Guid.Parse(_contractTesterSettings.ValidOrderCompletedContractId),
                Message = new OrderCompleteMessageDetailsWithAdditionalField
                {
                    Id = Guid.NewGuid(),
                    Timestamp = DateTime.Now,
                    InvalidField = "this should error out"
                }
            };

            var stringContent = new StringContent(JsonConvert.SerializeObject(placeOrderMessage), Encoding.UTF8, "application/json");
            var response = await Client.PostAsync(_contractTesterSettings.TestMessageApiUrl, stringContent);
            response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

            var responseString = await response.Content.ReadAsStringAsync();
            responseString.ShouldContain("Message property \\\"InvalidField\\\" is not part of the contract.");
        }
    }

    public class OrderCompletedContract : IContractTesterTemplate
    {
        public Guid ContractId { get; set; }
        public IMessageDetails Message { get; set; }
    }

    public class OrderCompleteMessageDetails : IMessageDetails
    {
        public string OrderId { get; set; }
        public Guid? Id { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class OrderCompleteMessageDetailsWithAdditionalField : IMessageDetails
    {
        public string InvalidField { get; set; }
        public Guid? Id { get; set; }
        public DateTime Timestamp { get; set; }
    }
}