namespace Integration.Service.Threads
{
    public interface IThreadHelper
    {
        T InvokeThreadSafe<T>(string key, int? expirySeconds, T defaultValue, Func<T> action);
    }
}
