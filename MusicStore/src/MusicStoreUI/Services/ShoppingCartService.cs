using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MusicStoreUI.Models;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using Steeltoe.Common.Discovery;

namespace MusicStoreUI.Services
{
    public class ShoppingCartService : IShoppingCart
    {
        private const string SHOPPINGCART_URL = "{cartId}";
        private const string SHOPPINGCART_ITEM_URL ="{cartId}/Item/{itemId}";

        private readonly HttpClient _httpClient;

        public ShoppingCartService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<bool> EmptyCartAsync(string cartId)
        {
            var url = SHOPPINGCART_URL.Replace("{cartId}", cartId);
            var response = await _httpClient.DeleteAsync(url);
            response.EnsureSuccessStatusCode();
            return true;
        }

        public async Task<List<CartItem>> GetCartItemsAsync(string cartId)
        {
            var url = SHOPPINGCART_URL.Replace("{cartId}", cartId);
            var response = await _httpClient.GetAsync(url);
            var result = CartItem.From(await response.Content.ReadAsAsync<List<CartItemJson>>());
            return result;
        }

        public async Task<bool> RemoveItemAsync(string cartId, int itemKey)
        {
            var url = SHOPPINGCART_ITEM_URL.Replace("{cartId}", cartId).Replace("{itemId}", itemKey.ToString());
            var response = await _httpClient.DeleteAsync(url);
            response.EnsureSuccessStatusCode();
            return true;
        }

        public async Task<bool> AddItemAsync(string cartId, int itemKey)
        {
            var url = SHOPPINGCART_ITEM_URL.Replace("{cartId}", cartId).Replace("{itemId}", itemKey.ToString());
            var response = await _httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Put, url));
            response.EnsureSuccessStatusCode();
            return true;
        }

        public async Task<bool> CreateCartAsync(string cartId)
        {
            var url = SHOPPINGCART_URL.Replace("{cartId}", cartId);
            var response = await _httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Put, url));
            response.EnsureSuccessStatusCode();
            return true;
        }
    }
}
