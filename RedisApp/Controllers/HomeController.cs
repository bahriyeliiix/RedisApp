using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

public class HomeController : Controller
{
    private readonly IConnectionMultiplexer _redis;

    public HomeController(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    public async Task<IActionResult> Counter()
    {
        var db = _redis.GetDatabase();
        await db.StringIncrementAsync("page_views");
        var viewCount = await db.StringGetAsync("page_views");
        ViewBag.ViewCount = viewCount;
        return View();
    }
}
