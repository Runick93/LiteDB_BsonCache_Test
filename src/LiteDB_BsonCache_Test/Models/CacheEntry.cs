using LiteDB;

namespace LiteDB_BsonCache_Test.Models
{
    internal class CacheEntry
    {
        [BsonId]
        public string Id { get; set; } = default!;
        public BsonDocument Data { get; set; } = default!;
        public DateTime CreatedAt { get; set; }
        public DateTime ExpireAt { get; set; }
    }
}
