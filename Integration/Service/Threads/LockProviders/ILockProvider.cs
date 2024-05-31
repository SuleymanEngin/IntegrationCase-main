namespace Integration.Service.Threads.LockProviders
{
    public interface ILockProvider
    {
        bool AcquireLock(string key, int? expirySeconds);
        void ReleaseLock(string key);
    }
}
