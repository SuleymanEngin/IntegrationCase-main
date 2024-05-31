using Integration.Service.Threads.LockProviders;

namespace Integration.Service.Threads
{
    public class ThreadHelper : IThreadHelper
    {
        public readonly ILockProvider LockProvider;
        public ThreadHelper(ILockProvider lockProvider)
        {
            LockProvider = lockProvider;
        }

        public T InvokeThreadSafe<T>(string key, int? expirySeconds, T defaultValue, Func<T> action)
        {
            var acquired = LockProvider.AcquireLock(key, expirySeconds);
            if (!acquired)
                return defaultValue;

            try
            {
                var result = action();
                return result;

            } catch {
                // release lock if action failed to let following call with same key to  be executed
                LockProvider.ReleaseLock(key);
                throw;
            }
        }

        public override string ToString()
        {
            return $"ThreadHelper: {LockProvider}";
        }
    }
}
