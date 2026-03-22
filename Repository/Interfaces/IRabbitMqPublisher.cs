namespace Repository;


public interface IRabbitMqPublisher
{
    Task PublishAsync(NotifcationMessage message);
}

