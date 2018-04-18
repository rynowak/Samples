using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MusicStoreUI.Models;
using System.Net.Http;
using Steeltoe.Common.Discovery;

namespace MusicStoreUI.Services
{
    public class OrderProcessingService : IOrderProcessing
    {
        private const string ORDER_URL = "/";

        private readonly HttpClient _httpClient;

        public OrderProcessingService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<int> AddOrderAsync(Order order)
        {
            var response = await _httpClient.PostAsJsonAsync(ORDER_URL, OrderJson.From(order));
            var result = await response.Content.ReadAsAsync<OrderJson>();
            return result.OrderId;
        }

        public async Task<Order> GetOrderAsync(int id)
        {
            var response = await _httpClient.GetAsync($"{ORDER_URL}?id={id}");
            var result = Order.From(await response.Content.ReadAsAsync<OrderJson>());

            foreach (var detail in result.OrderDetails)
            {
                detail.Order = result;
            }

            return result;
        }
    }
}
