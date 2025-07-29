using LiteDB;
using LiteDB_BsonCache_Test.Models;

namespace LiteDB_BsonCache_Test.Application
{
    internal class CacheManager
    {
        private readonly string _dbPath;
        private readonly string _collectionName;
        private readonly LiteDatabase _db;
        private readonly ILiteCollection<CacheEntry> _collection;

        public CacheManager(string dbPath, string collectionName)
        {
            _dbPath = dbPath;
            _collectionName = collectionName;

            var dir = Path.GetDirectoryName(_dbPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            // Crea la base de datos si no existe (LiteDB lo hace solo)
            _db = new LiteDatabase(_dbPath);

            // Obtiene o crea la colección automáticamente
            _collection = _db.GetCollection<CacheEntry>(_collectionName);

            // Asegura los índices
            _collection.EnsureIndex(x => x.Id);
            _collection.EnsureIndex(x => x.ExpireAt);
        }


        public void Set(string key, string json, TimeSpan timeToLive)
        {
            var doc = JsonSerializer.Deserialize(json).AsDocument;
            var entry = new CacheEntry
            {
                Id = key,
                Data = doc,
                CreatedAt = DateTime.UtcNow,
                ExpireAt = DateTime.UtcNow.Add(timeToLive)
            };
            _collection.Upsert(entry);
        }

        public CacheEntry? Get(string key)
        {
            return _collection.FindOne(x => x.Id == key);
        }


        // Funciones esporadicas.
        public void ClearExpired()
        {
            _collection.DeleteMany(x => x.ExpireAt <= DateTime.UtcNow);
        }

        public void Remove(string key)
        {
            _collection.Delete(key);
        }
    }
}
