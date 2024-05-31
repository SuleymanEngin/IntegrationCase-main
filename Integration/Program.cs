using Integration.Service;
using Integration.Service.Threads;
using Integration.Service.Threads.LockProviders;
using System.Collections.Concurrent;

namespace Integration;

public abstract class Program
{
    public static async Task Main(string[] args)
    {
        Console.Clear();

        var service = CreateItemIntegrationService();
        
        // generate data
        var contents = new List<string>();
        var valueSet = new string[] { "a", "b", "c", "d", "e", "f" };
        for (var i = 0; i < 50; i++) {
            var value = valueSet[Random.Shared.NextInt64(0, valueSet.Length)];
            contents.Add(value);
        }

        // print original data        
        Console.WriteLine("Original Data:");
        contents.ForEach(d => Console.Write($"{d} "));
        Console.WriteLine();
        Console.WriteLine();

        // print saved data by save time order
        Console.WriteLine("Saved Data: (line per ms group)");
        Thread.Sleep(500);

        var watch = System.Diagnostics.Stopwatch.StartNew();        
        var logs = new ConcurrentDictionary<decimal, List<string>>();
        
        await Parallel.ForEachAsync(
            contents,
            new ParallelOptions() { MaxDegreeOfParallelism = 3 },
            (content, _) =>
        {
            try
            {
                logs.GetOrAdd((int)Math.Round(watch.ElapsedTicks / 10_000.0, 0), new List<string>()).Add(content);

                service.SaveItem(content);
                Thread.Sleep(5);

            } catch (Exception e) {
                Console.WriteLine();
                Console.WriteLine($"Failed! Content: {content} => {e.Message}");
            }

            return new ValueTask();
        });

        logs.OrderBy(g => g.Key).ToList().ForEach(l => Console.WriteLine($"{Math.Round(l.Key, 2), 4} ms: {string.Join(" ", l.Value)}"));
        Console.WriteLine();


        Console.WriteLine();
        Console.WriteLine($"Everything recorded in {watch.ElapsedMilliseconds} ms");
        Console.WriteLine();

        // print saved data from external service
        Console.WriteLine("List of Saved Data from External Service:");
        service.GetAllItems().ForEach(d => Console.Write($"{d} "));
        Console.WriteLine();
        
        Console.WriteLine();
        Console.WriteLine("Finished.");
        Console.ReadLine();
    }

    static ItemIntegrationService CreateItemIntegrationService() {
        // can be injected by any dependency injection framework
        //var distributedLockProvider = new DistributedLockProvider("127.0.0.1:6379");
        //var threadHelper = new ThreadHelper(distributedLockProvider);

        var singleServerlockProvider = new SingleServerCacheProvider();
        var threadHelper = new ThreadHelper(singleServerlockProvider);

        Console.WriteLine($"ThreadHelper: {threadHelper}");
        Console.WriteLine();

        var service = new ItemIntegrationService(threadHelper);
        return service;
    }
}