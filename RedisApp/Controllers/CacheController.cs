using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using StackExchange.Redis;

public class CacheController : Controller
{
    private readonly IConnectionMultiplexer _redis;

    public CacheController(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    public async Task<IActionResult> Index()
    {
        var db = _redis.GetDatabase();
        string cacheKey = "popular_products";
        var products = await db.StringGetAsync(cacheKey);

        if (products.IsNullOrEmpty)
        {
            products = JsonConvert.SerializeObject(new List<string> { "Product1", "Product2", "Product3" });
            await db.StringSetAsync(cacheKey, products, TimeSpan.FromMinutes(10));
        }

        var popularProducts = JsonConvert.DeserializeObject<List<string>>(products);
        return View(popularProducts);
    }

    public async Task<IActionResult> ClearCache()
    {
        var db = _redis.GetDatabase();
        await db.KeyDeleteAsync("popular_products");
        return RedirectToAction("Index");
    }
}
