using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json.Linq;
using Services.Interfaces;
using OrderProcessor = Services.Services.OrderProcessor;

namespace Tests;

public class OrderProcessorTests
{
    [Fact]
    public async Task ProcessOrdersAsync_ShouldCallServicesCorrectly()
    {
        var mockOrderService = new Mock<IOrderService>();
        var mockNotificationService = new Mock<INotificationService>();
        var mockLogger = new Mock<ILogger<OrderProcessor>>();

        var orders = new[]
        {
            JObject.Parse(
                "{ \"OrderId\": \"1\", \"Items\": [{ \"Status\": \"Delivered\", \"Description\": \"Item1\", \"deliveryNotification\": 0 }] }")
        };

        mockOrderService.Setup(s => s.FetchOrdersAsync()).ReturnsAsync(orders);

        var processor = new OrderProcessor(mockOrderService.Object, mockNotificationService.Object, mockLogger.Object);

        await processor.ProcessOrdersAsync();

        mockNotificationService.Verify(s => s.SendDeliveryNotificationAsync(It.IsAny<JToken>(), "1"), Times.Once);
        mockOrderService.Verify(s => s.UpdateOrderAsync(It.IsAny<JObject>()), Times.Once);
    }

    [Fact]
    public async Task ProcessOrdersAsync_ShouldNotCallServices_WhenNoOrdersReturned()
    {
        var mockOrderService = new Mock<IOrderService>();
        var mockNotificationService = new Mock<INotificationService>();
        var mockLogger = new Mock<ILogger<OrderProcessor>>();

        mockOrderService.Setup(s => s.FetchOrdersAsync()).ReturnsAsync(Array.Empty<JObject>());

        var processor = new OrderProcessor(mockOrderService.Object, mockNotificationService.Object, mockLogger.Object);

        await processor.ProcessOrdersAsync();


        mockNotificationService.Verify(s => s.SendDeliveryNotificationAsync(It.IsAny<JToken>(), It.IsAny<string>()),
            Times.Never);
        mockOrderService.Verify(s => s.UpdateOrderAsync(It.IsAny<JObject>()), Times.Never);
        mockLogger.Verify(
            l => l.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task ProcessOrdersAsync_ShouldProcessMultipleOrdersAndItems()
    {
        var mockOrderService = new Mock<IOrderService>();
        var mockNotificationService = new Mock<INotificationService>();
        var mockLogger = new Mock<ILogger<OrderProcessor>>();

        var orders = new[]
        {
            JObject.Parse(
                "{ \"OrderId\": \"1\", \"Items\": [{ \"Status\": \"Delivered\", \"Description\": \"Item1\", \"deliveryNotification\": 0 }] }"),
            JObject.Parse(
                "{ \"OrderId\": \"2\", \"Items\": [{ \"Status\": \"Delivered\", \"Description\": \"Item2\", \"deliveryNotification\": 0 }] }")
        };

        mockOrderService.Setup(s => s.FetchOrdersAsync()).ReturnsAsync(orders);

        var processor = new OrderProcessor(mockOrderService.Object, mockNotificationService.Object, mockLogger.Object);


        await processor.ProcessOrdersAsync();


        mockNotificationService.Verify(s => s.SendDeliveryNotificationAsync(It.IsAny<JToken>(), It.IsAny<string>()),
            Times.Exactly(2));
        mockOrderService.Verify(s => s.UpdateOrderAsync(It.IsAny<JObject>()), Times.Exactly(2));
    }


    [Fact]
    public async Task ProcessOrdersAsync_ShouldContinueProcessing_WhenNotificationFails()
    {
        var mockOrderService = new Mock<IOrderService>();
        var mockNotificationService = new Mock<INotificationService>();
        var mockLogger = new Mock<ILogger<OrderProcessor>>();

        var orders = new[]
        {
            JObject.Parse(
                "{ \"OrderId\": \"1\", \"Items\": [{ \"Status\": \"Delivered\", \"Description\": \"Item1\", \"deliveryNotification\": 0 }] }"),
            JObject.Parse(
                "{ \"OrderId\": \"2\", \"Items\": [{ \"Status\": \"Delivered\", \"Description\": \"Item2\", \"deliveryNotification\": 0 }] }")
        };

        mockOrderService.Setup(s => s.FetchOrdersAsync()).ReturnsAsync(orders);
        mockNotificationService.Setup(s => s.SendDeliveryNotificationAsync(It.IsAny<JToken>(), It.IsAny<string>()))
            .ThrowsAsync(new Exception("Notification failed"));

        var processor = new OrderProcessor(mockOrderService.Object, mockNotificationService.Object, mockLogger.Object);


        await processor.ProcessOrdersAsync();


        mockNotificationService.Verify(s => s.SendDeliveryNotificationAsync(It.IsAny<JToken>(), It.IsAny<string>()),
            Times.Exactly(2));
        mockOrderService.Verify(s => s.UpdateOrderAsync(It.IsAny<JObject>()),
            Times.Exactly(2)); // Still updates even if notification fails
        mockLogger.Verify(
            l => l.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task ProcessOrdersAsync_ShouldLogAppropriateMessages()
    {
        var mockOrderService = new Mock<IOrderService>();
        var mockNotificationService = new Mock<INotificationService>();
        var mockLogger = new Mock<ILogger<OrderProcessor>>();

        var orders = new[]
        {
            JObject.Parse(
                "{ \"OrderId\": \"1\", \"Items\": [{ \"Status\": \"Delivered\", \"Description\": \"Item1\", \"deliveryNotification\": 0 }] }")
        };

        mockOrderService.Setup(s => s.FetchOrdersAsync()).ReturnsAsync(orders);

        var processor = new OrderProcessor(mockOrderService.Object, mockNotificationService.Object, mockLogger.Object);


        await processor.ProcessOrdersAsync();


        mockLogger.Verify(
            l => l.Log(LogLevel.Information, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), null,
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.AtLeastOnce);
        mockLogger.Verify(
            l => l.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Never);
    }

    [Fact]
    public async Task ProcessOrdersAsync_ShouldLogError_WhenFetchOrdersFails()
    {
        var mockOrderService = new Mock<IOrderService>();
        var mockNotificationService = new Mock<INotificationService>();
        var mockLogger = new Mock<ILogger<OrderProcessor>>();

        mockOrderService.Setup(s => s.FetchOrdersAsync()).ThrowsAsync(new Exception("API failure"));

        var processor = new OrderProcessor(mockOrderService.Object, mockNotificationService.Object, mockLogger.Object);


        await processor.ProcessOrdersAsync();


        mockNotificationService.Verify(s => s.SendDeliveryNotificationAsync(It.IsAny<JToken>(), It.IsAny<string>()),
            Times.Never);
        mockOrderService.Verify(s => s.UpdateOrderAsync(It.IsAny<JObject>()), Times.Never);
        mockLogger.Verify(
            l => l.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Once);
    }
}