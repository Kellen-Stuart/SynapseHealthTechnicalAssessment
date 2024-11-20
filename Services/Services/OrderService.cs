using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Services.Interfaces;

namespace Services.Services;

public class OrderService : IOrderService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OrderService> _logger;
    private const string OrdersApiUrl = "https://orders-api.com/orders";
    private const string UpdateApiUrl = "https://update-api.com/update";

    public OrderService(HttpClient httpClient, ILogger<OrderService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<JObject[]> FetchOrdersAsync()
    {
        _logger.LogInformation("Fetching orders from API...");
        var response = await _httpClient.GetAsync(OrdersApiUrl);

        if (response.IsSuccessStatusCode)
        {
            var ordersData = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Successfully fetched orders.");
            return JArray.Parse(ordersData).ToObject<JObject[]>();
        }
        
        _logger.LogError("Failed to fetch orders from API. Status code: {StatusCode}", response.StatusCode);
        throw new HttpRequestException("Error fetching orders");
    }

    public async Task UpdateOrderAsync(JObject order)
    {
        _logger.LogInformation("Updating order with ID {OrderId}", order["OrderId"]);
        var response = await _httpClient.PostAsJsonAsync(UpdateApiUrl, order);

        if (response.IsSuccessStatusCode)
        {
            _logger.LogInformation("Order updated successfully for ID {OrderId}", order["OrderId"]);
        }
        else
        {
            _logger.LogError("Failed to update order for ID {OrderId}. Status code: {StatusCode}", order["OrderId"], response.StatusCode);
            throw new HttpRequestException("Error updating order");
        }
    }
}