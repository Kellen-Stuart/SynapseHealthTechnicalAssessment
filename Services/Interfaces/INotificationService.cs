using Newtonsoft.Json.Linq;

namespace Services.Interfaces;

public interface INotificationService
{
    Task SendDeliveryNotificationAsync(JToken item, string orderId);
}