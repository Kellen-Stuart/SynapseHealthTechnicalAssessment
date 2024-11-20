using Newtonsoft.Json.Linq;
using Services.Interfaces;

namespace Services.Services;
using Microsoft.Extensions.Logging;

public class OrderProcessor : IOrderProcessor
{
    private readonly IOrderService _orderService;
    private readonly INotificationService _notificationService;
    private readonly ILogger<OrderProcessor> _logger;

    public OrderProcessor(IOrderService orderService, INotificationService notificationService, ILogger<OrderProcessor> logger)
    {
        _orderService = orderService;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task ProcessOrdersAsync()
    {
        _logger.LogInformation("Starting order processing...");

        try
        {
            var orders = await _orderService.FetchOrdersAsync();
            foreach (var order in orders)
            {
                _logger.LogInformation("Processing order with ID {OrderId}", order["OrderId"]);

                var items = order["Items"].ToObject<JArray>();
                foreach (var item in items)
                {
                    if (IsItemDelivered(item))
                    {
                        try
                        {
                            await _notificationService.SendDeliveryNotificationAsync(item, order["OrderId"].ToString());
                            IncrementDeliveryNotification(item);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"Error sending delivery notification: {ex.Message}");
                        }
                    }
                }

                await _orderService.UpdateOrderAsync(order);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing orders.");
        }

        _logger.LogInformation("Order processing completed.");
    }

    private static bool IsItemDelivered(JToken item)
    {
        return item["Status"]?.ToString().Equals("Delivered", StringComparison.OrdinalIgnoreCase) ?? false;
    }

    private static void IncrementDeliveryNotification(JToken item)
    {
        item["deliveryNotification"] = item["deliveryNotification"]?.Value<int>() + 1 ?? 1;
    }
}
