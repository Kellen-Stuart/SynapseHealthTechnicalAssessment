using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Services.Interfaces;
using Services.Services;

class Program
{
    static async Task Main(string[] args)
    {
        using var host = CreateHostBuilder(args).Build();
        var orderProcessor = host.Services.GetRequiredService<IOrderProcessor>();

        await orderProcessor.ProcessOrdersAsync();
    }

    static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddConsole();
                logging.AddDebug();
            })
            .ConfigureServices((_, services) =>
            {
                services.AddHttpClient();
                services.AddScoped<IOrderService, OrderService>();
                services.AddScoped<IOrderProcessor, OrderProcessor>();
                services.AddScoped<INotificationService, NotificationService>();
            });
}