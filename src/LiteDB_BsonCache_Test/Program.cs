using LiteDB_BsonCache_Test.Application;
using LiteDB_BsonCache_Test.Models;

namespace LiteDB_BsonCache_Test
{
    internal class Program
    {
        private static CacheManager _cacheManager;

        static void Main(string[] args)
        {
            string?[] tables = Directory.GetFiles(@".\Tablas\", "*.json")
                .Select(Path.GetFileNameWithoutExtension)
                .ToArray();

            string dbPath = @".\Tablas\Cache.db";
            _cacheManager = new CacheManager(dbPath, "CacheEntries");

            foreach (string? table in tables)
            {
                string? result = GetTable(table);
                Console.WriteLine(result);
            }
        }

        public static string? GetTable(string? key)
        {
            const int secondsToExpireThreshold = 30;
            string? json = null;

            CacheEntry? entry = _cacheManager.Get(key);

            if (entry == null)
            {
                Console.WriteLine("No hay cache, creando nueva.");
                try
                {
                    json = CreateTableCache(key);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al crear cache: {ex.Message}");
                    return null;
                }
            }
            else
            {
                double remainingSeconds = (entry.ExpireAt - DateTime.UtcNow).TotalSeconds;

                if (remainingSeconds <= secondsToExpireThreshold)
                {                    
                    try
                    {
                        json = CreateTableCache(key);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error al refrescar, devolviendo cache vencida: {ex.Message}");
                        json = entry.Data.ToString();
                    }
                }
                else
                {
                    Console.WriteLine($"Usando cache existente (vence en {remainingSeconds:F0}s)");
                    json = entry.Data.ToString();
                }
            }

            return json;
        }

        public static string CreateTableCache(string key)
        {
            string filePath = @$".\Tablas\{key}.json";

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"El archivo de la tabla '{key}' no fue encontrado en: {filePath}");
            }

            string json = File.ReadAllText(filePath);
            _cacheManager.Set(key, json, TimeSpan.FromMinutes(10));
            return json;
        }
    }
}
