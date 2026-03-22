using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Repository;

public class NotificationConsumer : BackgroundService
{
    private readonly IConnection _connection;
    private readonly IServiceScopeFactory _scopeFactory; // ✅ not INotificationService directly

    public NotificationConsumer(IConnection connection, IServiceScopeFactory scopeFactory)
    {
        _connection = connection;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var channel = _connection.CreateModel();
        channel.QueueDeclare("notification_queue", durable: true, exclusive: false, autoDelete: false);

         var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.Received += async (model, ea) =>
        {
            try
            {
                using var scope = _scopeFactory.CreateScope(); // ✅ create scope per message
                var notificationService = scope.ServiceProvider.GetRequiredService<INotificationRedisService>();

                var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                var message = JsonSerializer.Deserialize<NotifcationMessage>(json);

                string finalMessage = message.Type switch
                {
                    "UserRegistered" => $"{message.Username} registered",
                    "QueryCreated" => $"{message.Username} created query: {message.QueryTitle}",
                    _ => "Unknown event"
                };

                await notificationService.AddNotificationAsync(finalMessage);
                channel.BasicAck(ea.DeliveryTag, multiple: false);
            }
            catch (Exception)
            {
                channel.BasicNack(ea.DeliveryTag, multiple: false, requeue: true);
            }
        };

        channel.BasicConsume("notification_queue", autoAck: false, consumer);
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}