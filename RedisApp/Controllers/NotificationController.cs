using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

public class NotificationController : Controller
{
    private readonly IConnectionMultiplexer _redis;

    public NotificationController(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    public IActionResult SendNotification()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> SendNotification(string message)
    {
        var sub = _redis.GetSubscriber();
        await sub.PublishAsync("notifications", message);
        return RedirectToAction("SendNotification");
    }

    public IActionResult ReceiveNotification()
    {
        var sub = _redis.GetSubscriber();
        sub.Subscribe("notifications", (channel, message) =>
        {
            TempData["Notification"] = message.ToString();
        });
        return View();
    }
}
