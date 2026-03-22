
using StackExchange.Redis;
namespace Repository;

public class NotificationRedisService : INotificationRedisService
{
    private readonly IDatabase _redisDb;
    private const string KEY = "admin_notifications";
    

    public NotificationRedisService(IConnectionMultiplexer redis)
    {
        _redisDb = redis.GetDatabase();
    }

    // ✅ Add notification
    public async Task AddNotificationAsync(string message)
    {
        await _redisDb.ListLeftPushAsync(KEY, message);


         
    }

    // ✅ Get latest notifications
    public async Task<List<string>> GetNotificationsAsync()
    {
        var data = await _redisDb.ListRangeAsync(KEY, 0, 20);
        return data.Select(x => x.ToString()).ToList();
    }

    // ✅ Get count for bell icon
    public async Task<long> GetNotificationCountAsync()
    {
        return await _redisDb.ListLengthAsync(KEY);
    }

    // ✅ Clear all notifications
    public async Task ClearAllAsync()
    {
        await _redisDb.KeyDeleteAsync(KEY);
    }
}
