using Basket.API.Entities;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Basket.API.Repositories
{
    public class BasketRepository : IBasketRepository
    {
        private readonly IDistributedCache redisCache;

        public BasketRepository(IDistributedCache redisCache)
        {
            this.redisCache = redisCache ?? throw new ArgumentNullException(nameof(redisCache));
        }

        public async Task DeleteBasket(string userName)
        {
            await redisCache.RemoveAsync(userName);
        }

        public async Task<ShoppingCart> GetBasket(string userName)
        {
            var basket = await redisCache.GetStringAsync(userName);
            if (string.IsNullOrWhiteSpace(basket)){
                return null;
            }
            return JsonConvert.DeserializeObject<ShoppingCart>(basket);
        }

        public async Task<ShoppingCart> UpdateBasket(ShoppingCart shoppingCart)
        {
            await redisCache.SetStringAsync(shoppingCart.Username,JsonConvert.SerializeObject(shoppingCart));
            return await GetBasket(shoppingCart.Username);
        }
    }
}
