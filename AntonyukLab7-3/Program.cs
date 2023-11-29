using System;
using System.Collections.Generic;

public class FunctionCache<TKey, TResult>
{
    private readonly Dictionary<TKey, CacheItem> cache = new Dictionary<TKey, CacheItem>();
    private readonly object lockObject = new object();

    // Делегат для користувацьких функцій
    public delegate TResult FuncDelegate(TKey key);

    // Кешований елемент
    private class CacheItem
    {
        public TResult Result { get; set; }
        public DateTime ExpirationTime { get; set; }
    }

    // Метод для отримання результату виклику функції з кешу
    public TResult GetOrAdd(TKey key, FuncDelegate func, TimeSpan expirationTime)
    {
        lock (lockObject)
        {
            if (cache.TryGetValue(key, out var cachedItem) && DateTime.Now < cachedItem.ExpirationTime)
            {
                // Повертаємо кешований результат
                return cachedItem.Result;
            }
            else
            {
                // Викликаємо користувацьку функцію
                TResult result = func(key);

                // Зберігаємо результат в кеші з терміном дії
                cache[key] = new CacheItem { Result = result, ExpirationTime = DateTime.Now.Add(expirationTime) };

                return result;
            }
        }
    }
}

class Program
{
    static void Main()
    {
        // Приклад використання кешу з функцією, що повертає довжину рядка
        FunctionCache<string, int> lengthCache = new FunctionCache<string, int>();

        FunctionCache<string, int>.FuncDelegate calculateLength = s =>
        {
            Console.WriteLine($"Calculating length for '{s}'");
            return s.Length;
        };

        // Перший виклик, викликає функцію і зберігає результат в кеші
        int length1 = lengthCache.GetOrAdd("apple", calculateLength, TimeSpan.FromSeconds(5));
        Console.WriteLine($"Length of 'apple': {length1}");

        // Другий виклик, повертає результат з кешу
        int length2 = lengthCache.GetOrAdd("apple", calculateLength, TimeSpan.FromSeconds(5));
        Console.WriteLine($"Length of 'apple' (from cache): {length2}");

        // Затримка, щоб термін дії кешу минув
        System.Threading.Thread.Sleep(6000);

        // Третій виклик, знову викликає функцію, оскільки термін дії кешу минув
        int length3 = lengthCache.GetOrAdd("apple", calculateLength, TimeSpan.FromSeconds(5));
        Console.WriteLine($"Length of 'apple' (after expiration): {length3}");
    }
}
