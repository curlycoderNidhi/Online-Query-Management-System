using Microsoft.AspNetCore.Mvc;
using Repository;

namespace MVC
{
    public class NotificationController : Controller
{
    private readonly INotificationRedisService _redisService;

    public NotificationController(INotificationRedisService redisService)
    {
        _redisService = redisService;
    }

       public async Task<IActionResult> GetAll()
    {
        var notifications = await _redisService.GetNotificationsAsync();
        return Json(notifications);
    }

   
    public async Task<IActionResult> Count()
    {
        var count = await _redisService.GetNotificationCountAsync();
        return Json(new { count });
    }

    
    [HttpPost]
    public async Task<IActionResult> Clear()
    {
        await _redisService.ClearAllAsync();
        return Json(new { success = true });
    }
}
}
