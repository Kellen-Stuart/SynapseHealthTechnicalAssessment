using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Services.Interfaces;

namespace Services.Services;

public class NotificationService : INotificationService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<NotificationService> _logger;
    private const string AlertApiUrl = "https://alert-api.com/alerts";

    public NotificationService(HttpClient httpClient, ILogger<NotificationService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task SendDeliveryNotificationAsync(JToken item, string orderId)
    {
        _logger.LogInformation("Sending delivery notification for Order {OrderId}, Item {ItemDescription}", orderId, item["Description"]);

        var alertData = new
        {
            Message = $"Alert for delivered item: Order {orderId}, Item: {item["Description"]}, " +
                      $"Delivery Notifications: {item["deliveryNotification"]}"
        };

        var response = await _httpClient.PostAsJsonAsync(AlertApiUrl, alertData);

        if (response.IsSuccessStatusCode)
        {
            _logger.LogInformation("Notification sent successfully for Order {OrderId}, Item {ItemDescription}", orderId, item["Description"]);
        }
        else
        {
            _logger.LogError("Failed to send notification for Order {OrderId}, Item {ItemDescription}. Status code: {StatusCode}", orderId, item["Description"], response.StatusCode);
            throw new HttpRequestException("Error sending notification");
        }
    }
}