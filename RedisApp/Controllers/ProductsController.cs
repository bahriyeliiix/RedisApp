using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using RedisApp.Models;
using System.Collections.Generic;
using System.Linq;

namespace RedisApp.Controllers
{
    public class ProductController : Controller
    {
        private readonly IConnectionMultiplexer _redis;

        public ProductController(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        public IActionResult AddProduct()
        {
            return View();
        }
        [HttpPost]
        public IActionResult AddProduct(Product product)
        {
            var db = _redis.GetDatabase();
            var redisKey = "popular_products";

            // Generate a new sequential ID
            var productId = (int)db.StringIncrement("product_id_counter");
            product.Id = productId;

            // Ensure the sorted set key exists
            if (db.KeyType(redisKey) != RedisType.SortedSet)
            {
                db.KeyDelete(redisKey);
            }

            // Add the new product to Redis
            db.SortedSetAdd(redisKey, product.Name, product.PopularityScore);
            db.HashSet($"product:{product.Id}", new HashEntry[]
            {
        new HashEntry("Name", product.Name),
        new HashEntry("Price", product.Price.ToString()),
        new HashEntry("Description", product.Description)
            });

            return RedirectToAction("Index");
        }

        public IActionResult Index(string searchTerm = "")
        {
            var products = string.IsNullOrWhiteSpace(searchTerm)
                ? GetAllProducts()  // If no search term, list all products
                : SearchProductsByName(searchTerm);  // If search term exists, filter products

            ViewBag.SearchTerm = searchTerm;  // Pass the search term to the view
            return View(products);
        }
        private List<Product> GetAllProducts()
        {
            var db = _redis.GetDatabase();
            var server = _redis.GetServer("localhost", 6379);
            var keys = server.Keys(pattern: "product:*");

            return keys.Select(key => GetProductFromRedis(key)).Where(product => product != null).ToList();
        }
        public IActionResult PopularProductList()
        {
            var db = _redis.GetDatabase();
            var redisKey = "popular_products";

            var popularProducts = db.SortedSetRangeByRank(redisKey, 0, -1, Order.Descending);
            var products = popularProducts.Select(p => p.ToString()).ToList();

            return View(products);
        }
        public IActionResult Search(string searchTerm)
        {
            var products = SearchProductsByName(searchTerm);
            return View(products);
        }
        public IActionResult EditProduct(int id)
        {
            var product = GetProductById(id);
            return product == null ? NotFound() : View(product);
        }
        [HttpPost]
        public IActionResult DeleteProduct(int id)
        {
            var db = _redis.GetDatabase();
            var redisKey = "popular_products";

            // Remove product from Redis
            db.SortedSetRemove(redisKey, $"product:{id}");
            db.KeyDelete($"product:{id}");

            return RedirectToAction("Index");
        }
        private List<Product> SearchProductsByName(string searchTerm)
        {
            var db = _redis.GetDatabase();
            var server = _redis.GetServer("localhost", 6379);
            var keys = server.Keys(pattern: "product:*");

            return keys
                .Select(key => GetProductFromRedis(key))
                .Where(product => product != null && product.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
        private Product GetProductById(int id)
        {
            return GetProductFromRedis($"product:{id}");
        }
        private Product GetProductFromRedis(string key)
        {
            var db = _redis.GetDatabase();

            if (!db.KeyExists(key))
                return null;

            return new Product
            {
                Id = int.Parse(key.Split(':')[1]),
                Name = db.HashGet(key, "Name"),
                Price = decimal.Parse(db.HashGet(key, "Price")),
                Description = db.HashGet(key, "Description")
            };
        }
        [HttpPost]
        public IActionResult EditProduct(Product product)
        {
            var db = _redis.GetDatabase();
            var key = $"product:{product.Id}";

            db.HashSet(key, new HashEntry[]
            {
        new HashEntry("Name", product.Name),
        new HashEntry("Price", product.Price.ToString()),
        new HashEntry("Description", product.Description)
            });

            return RedirectToAction("Index");
        }




    }
}
