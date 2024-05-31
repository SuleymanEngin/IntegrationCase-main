using System.Runtime.Caching;

namespace Integration.Service.Threads.LockProviders
{
    public class SingleServerCacheProvider : ISingleServerLockProvider
    {
        // This is a cache that is used to lock the item content.
        // MemoryCache have chosen over ConcurrentDictionary because it has a built-in expiration mechanism.
        public static readonly MemoryCache locks = new MemoryCache("locks");
        private readonly object internalLockObject = new object();

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

            lock (internalLockObject)
            {
                bool acquired = false;
                if (locks.Get(key) == null)
                {
                    acquired = true;
                    var expiryTime = expirySeconds.HasValue ? DateTimeOffset.Now.AddSeconds(expirySeconds.Value) : DateTimeOffset.MaxValue;
                    locks.Add(key, true, expiryTime);
                }

                return acquired;
            }
        }

        /// <summary>
        /// Explicitly release lock for given key
        /// </summary>
        /// <param name="key">Lock Key</param>
        public void ReleaseLock(string key) {
            lock (internalLockObject)
            {
                locks.Remove(key);
            }
        }

        public override string ToString()
        {
            return $"SingleServerCacheProvider";
        }
    }
}
