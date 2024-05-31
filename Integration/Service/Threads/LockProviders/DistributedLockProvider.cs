using StackExchange.Redis;

namespace Integration.Service.Threads.LockProviders
{
    public class DistributedLockProvider : IDistributedLockProvider
    {
        private readonly string connectionString;
        private IDatabase database;

        public DistributedLockProvider(string connectionString)
        {
            this.connectionString = connectionString;
        }

        /// <summary>
        /// Open redis connection and gets database
        /// </summary>
        /// <returns>References to Redis database</returns>
        private IDatabase GetDatabase() {            
            if (this.database == null)
            {
                var redis = ConnectionMultiplexer.Connect(this.connectionString);
                this.database = redis.GetDatabase(0);
            }

            return this.database;
        }

        /// <summary>
        /// Get the lock object from the cache
        /// </summary>
        /// <param name="key">Lock Key. Cannot be null!</param>
        /// <param name="expirySeconds">Expiry duration in seconds. Null value means no expiry.</param>
        /// <returns>True is lock is acquired. False if it is already locked.</returns>
        public bool AcquireLock(string key, int? expirySeconds)
        {
            if (key == null)
                throw new ArgumentNullException("key is null.");

            var db = GetDatabase();
            var expiryTime = expirySeconds.HasValue ? TimeSpan.FromSeconds(expirySeconds.Value) : TimeSpan.MaxValue;
            var acquired = db.StringSet(key, true, expiryTime, false, When.NotExists);
            return acquired;
        }

        /// <summary>
        /// Explicitly release lock for given key
        /// </summary>
        /// <param name="key">Lock Key</param>
        public void ReleaseLock(string key)
        {
            var db = GetDatabase();
            db.StringGetDelete(key);
        }

        public override string ToString()
        {
            return $"DistributedLockProvider: {connectionString}";
        }
    }
}
