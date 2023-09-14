using RedisExampleApp.API.Models;
using RedisExampleApp.Cache;
using StackExchange.Redis;
using System.Text.Json;

namespace RedisExampleApp.API.Repositories
{
    public class ProductRepositoryWithCacheDecorator : IProductRepository
    {
        private const string productKey = "productCaches"; //Hash kullandık..tablomun hashsetine bir isim vericem.. Bu benim redisde tutacağım cache ismim.
        private readonly IProductRepository _productRepository;
        private readonly RedisService _redisService;
        private readonly IDatabase _cacheRepository;


        public ProductRepositoryWithCacheDecorator(IProductRepository repository, RedisService redisService)
        {
            _productRepository = repository;
            _redisService = redisService;
            _cacheRepository = _redisService.GetDb(2); //2. db yi alıyoruz
        }

        public async Task<Product> CreateAsync(Product product)
        {
            var newProduct = await _productRepository.CreateAsync(product);  //DB

            if(await _cacheRepository.KeyExistsAsync(productKey)) //Cache
            {
                await _cacheRepository.HashSetAsync(productKey,product.Id,JsonSerializer.Serialize(newProduct));
            }
            return newProduct;
        }

        public async Task<List<Product>> GetAsync()
        {
            if(!await _cacheRepository.KeyExistsAsync(productKey)) //data cache de değil ise cache leyip dönüyor 
                return await LoadToCacheFromDbAsync();

            var products = new List<Product>();
            var cacheProducts = await _cacheRepository.HashGetAllAsync(productKey);
            foreach (var item in cacheProducts.ToList()) // artık data memory de olduğu için ToListAync yerine ToList kullandık
            {
                var product = JsonSerializer.Deserialize<Product>(item.Value);
                products.Add(product);
            }

            return products;
        }

        public async Task<Product> GetByIdAsync(int id)
        {
            if(_cacheRepository.KeyExists(productKey))
            {
                var product = await _cacheRepository.HashGetAsync(productKey, id);
                return product.HasValue ? JsonSerializer.Deserialize<Product>(product) : null;
            }

            var products = await LoadToCacheFromDbAsync();
            return products.FirstOrDefault(x => x.Id == id);
        }

        private async Task<List<Product>> LoadToCacheFromDbAsync() //bu metot db den datayı cacheleyecek
        {
            var products = await _productRepository.GetAsync();

            //db den datayı cacheleme yapıyoruz
            products.ForEach(p =>
            {
                //her foreach ile dönerek aynı productKey'e key değeriyle beraber bu key e karşılık gelen tüm product datasını serialize ediyorum.
                //bu sayede Id ile redisden datayı çok hızlı çekeriz.
                _cacheRepository.HashSetAsync(productKey, p.Id, JsonSerializer.Serialize(p));
            });

            return products;
        }
    }
}
