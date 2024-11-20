using Newtonsoft.Json.Linq;

namespace Services.Interfaces;

public interface IOrderService
{
    Task<JObject[]> FetchOrdersAsync();
    Task UpdateOrderAsync(JObject order);
}