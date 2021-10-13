using Microsoft.eShopOnContainers.Services.Basket.API.Model;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;

namespace Microsoft.eShopOnContainers.Services.Basket.API.Infrastructure.Repositories
{
    public class RedisBasketRepository : IBasketRepository
    {
        private readonly ILogger<RedisBasketRepository> _logger;
        private readonly ConnectionMultiplexer _redis;
        private readonly IDatabase _database;

        public RedisBasketRepository(ILoggerFactory loggerFactory, ConnectionMultiplexer redis)
        {
            _logger = loggerFactory.CreateLogger<RedisBasketRepository>();
            _redis = redis;
            _database = redis.GetDatabase();
        }

        public async Task<bool> DeleteBasketAsync(string id)
        {
            return await _database.KeyDeleteAsync(id);
        }

        public IEnumerable<string> GetUsers()
        {
            var server = GetServer();
            var data = server.Keys();

            return data?.Select(k => k.ToString());
        }

        public async Task<CustomerBasket> GetBasketAsync(string customerId)
        {
            //var data = await _database.StringGetAsync(customerId);

            //if (data.IsNullOrEmpty)
            //{
            //    return null;
            //}

            //return JsonSerializer.Deserialize<CustomerBasket>(data, new JsonSerializerOptions
            //{
            //    PropertyNameCaseInsensitive = true
            //});
            var x = new List<BasketItem>();
            x.Add(new BasketItem() { Id = "2222", ProductId=333, ProductName="trà", UnitPrice = 123,OldUnitPrice=12,PictureUrl="google.comz",Quantity=2 });
            var data = new CustomerBasket();
            data.BuyerId = "xxx";
            data.Items = x;

            var hani = JsonSerializer.Serialize<CustomerBasket>(data);

            return JsonSerializer.Deserialize<CustomerBasket>(hani, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }

        public async Task<CustomerBasket> UpdateBasketAsync(CustomerBasket basket)
        {
            var created = await _database.StringSetAsync(basket.BuyerId, JsonSerializer.Serialize(basket));

            if (!created)
            {
                _logger.LogInformation("Problem occur persisting the item.");
                return null;
            }

            _logger.LogInformation("Basket item persisted succesfully.");

            return await GetBasketAsync(basket.BuyerId);
        }

        private IServer GetServer()
        {
            var endpoint = _redis.GetEndPoints();
            return _redis.GetServer(endpoint.First());
        }
    }
}
