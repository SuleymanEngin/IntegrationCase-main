using StackExchange.Redis;
using Integration.Common;
using Integration.Backend;
using System;

namespace Integration.Service
{
    public sealed class ItemIntegrationService
    {
        private readonly ItemOperationBackend ItemIntegrationBackend = new();
        private static readonly ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("localhost");
        private static readonly IDatabase db = redis.GetDatabase();

        public Result SaveItem(string itemContent)
        {
            var lockKey = $"lock:{itemContent}";
            
            // Try to acquire the lock
            if (db.LockTake(lockKey, Environment.MachineName, TimeSpan.FromSeconds(1000)))
            {
                try
                {
                    if (ItemIntegrationBackend.FindItemsWithContent(itemContent).Count != 0)
                    {
                        return new Result(false, $"Duplicate item received with content {itemContent}.");
                    }

                    var item = ItemIntegrationBackend.SaveItem(itemContent);
                    return new Result(true, $"Item with content {itemContent} saved with id {item.Id}");
                }
                finally
                {
                    db.LockRelease(lockKey, Environment.MachineName);
                }
            }
            else
            {
                return new Result(false, $"Could not acquire lock for content {itemContent}.");
            }
        }

        public List<Item> GetAllItems()
        {
            return ItemIntegrationBackend.GetAllItems();
        }
    }
}
