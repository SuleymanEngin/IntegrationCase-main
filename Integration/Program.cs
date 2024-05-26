using Integration.Service;
using System;
using System.Threading;

namespace Integration
{
    public abstract class Program
    {
        public static void Main(string[] args)
        {
            var service = new ItemIntegrationService();

            // İlk grup istekler
            ThreadPool.QueueUserWorkItem(_ =>
            {
                var result = service.SaveItem("a");
                Console.WriteLine(result.Message);
            });
            ThreadPool.QueueUserWorkItem(_ =>
            {
                var result = service.SaveItem("b");
                Console.WriteLine(result.Message);
            });
            ThreadPool.QueueUserWorkItem(_ =>
            {
                var result = service.SaveItem("c");
                Console.WriteLine(result.Message);
            });

            Thread.Sleep(500);

            // İkinci grup istekler
            ThreadPool.QueueUserWorkItem(_ =>
            {
                var result = service.SaveItem("a");
                Console.WriteLine(result.Message);
            });
            ThreadPool.QueueUserWorkItem(_ =>
            {
                var result = service.SaveItem("b");
                Console.WriteLine(result.Message);
            });
            ThreadPool.QueueUserWorkItem(_ =>
            {
                var result = service.SaveItem("c");
                Console.WriteLine(result.Message);
            });

            Thread.Sleep(5000);

            Console.WriteLine("Everything recorded:");

            var allItems = service.GetAllItems();
            foreach (var item in allItems)
            {
                Console.WriteLine($"Item ID: {item.Id}, Content: {item.Content}");
            }

            Console.ReadLine();
        }
    }
}
