namespace RedisApp.Models
{
    public class Product
    {
        public int Id { get; set; }   // Ürün ID'si
        public string Name { get; set; }  // Ürün adı
        public decimal Price { get; set; }  // Ürün fiyatı
        public string Description { get; set; }  // Ürün açıklaması
        public double PopularityScore { get; set; }  // Popülerlik puanı
    }
}
