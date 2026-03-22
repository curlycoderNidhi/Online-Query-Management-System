namespace Repository;

public interface INotificationRedisService
{
  Task AddNotificationAsync(string message);
    Task<List<string>> GetNotificationsAsync();
    Task<long> GetNotificationCountAsync();
    Task ClearAllAsync();
}
