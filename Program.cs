using System;
using System.Collections.Generic;
using System.Threading;

public class FunctionCache<TKey, TResult>
{
    private readonly Func<TKey, TResult> _function;
    private readonly Dictionary<TKey, (TResult Result, DateTime Expiry)> _cache;
    private readonly TimeSpan _cacheDuration;

    public FunctionCache(Func<TKey, TResult> function, TimeSpan cacheDuration)
    {
        _function = function ?? throw new ArgumentNullException(nameof(function));
        _cache = new Dictionary<TKey, (TResult, DateTime)>();
        _cacheDuration = cacheDuration;
    }

    public TResult Get(TKey key)
    {
        if (_cache.ContainsKey(key))
        {
            var cacheEntry = _cache[key];
            if (DateTime.Now <= cacheEntry.Expiry)
            {
                // Return cached result if it is still valid
                Console.WriteLine($"Cache hit for key: {key}");
                return cacheEntry.Result;
            }
            else
            {
                // Remove expired cache entry
                _cache.Remove(key);
                Console.WriteLine($"Cache expired for key: {key}");
            }
        }

        // If the key is not in the cache or the cache has expired, execute the function
        Console.WriteLine($"Executing function for key: {key}...");
        TResult result = _function(key);
        _cache[key] = (result, DateTime.Now + _cacheDuration);
        return result;
    }
}

class Program
{
    static void Main()
    {
        Console.WriteLine("Welcome to the Function Cache demo!");

        // Create a function that simulates an expensive operation
        Func<int, int> expensiveFunction = key =>
        {
            Thread.Sleep(2000); // Simulate a delay
            return key * key;
        };

        // Get the cache duration from the user
        Console.WriteLine("Enter the cache duration in seconds:");
        if (!int.TryParse(Console.ReadLine(), out int cacheDurationSeconds) || cacheDurationSeconds <= 0)
        {
            Console.WriteLine("Invalid cache duration. Please enter a positive integer.");
            return;
        }

        // Create a cache with the specified duration
        FunctionCache<int, int> cache = new FunctionCache<int, int>(expensiveFunction, TimeSpan.FromSeconds(cacheDurationSeconds));

        while (true)
        {
            Console.WriteLine("Enter a key to get the result (or type 'exit' to quit):");
            string input = Console.ReadLine();
            if (input.ToLower() == "exit")
                break;

            if (int.TryParse(input, out int key))
            {
                // Get the result from the cache
                int result = cache.Get(key);
                Console.WriteLine($"Result for key {key}: {result}");
            }
            else
            {
                Console.WriteLine("Invalid input. Please enter an integer key.");
            }
        }
    }
}
