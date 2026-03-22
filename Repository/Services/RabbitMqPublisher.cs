
using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

namespace Repository;

public class RabbitMqPublisher : IRabbitMqPublisher
{


private readonly IConnection _conn;

public RabbitMqPublisher(IConnection conn)
{
    _conn = conn;

    // Setup only once using a temp channel
    using var setupChannel = _conn.CreateModel();
    setupChannel.ExchangeDeclare("notification_exchange", ExchangeType.Direct);
     setupChannel.QueueDeclare("notification_queue", durable: true, exclusive: false, autoDelete: false);
    setupChannel.QueueBind("notification_queue", "notification_exchange", "notification_key");
}

public Task PublishAsync(NotifcationMessage message)
{
    using var channel = _conn.CreateModel(); // fresh channel per call
    var json = JsonSerializer.Serialize(message);
    var body = Encoding.UTF8.GetBytes(json);

    channel.BasicPublish(
        exchange: "notification_exchange",
        routingKey: "notification_key",
        body: body
    );

    return Task.CompletedTask;
}
    
}
