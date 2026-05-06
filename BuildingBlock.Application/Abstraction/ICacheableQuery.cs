using System.Text.Json.Serialization;

namespace BuildingBlock.Application.Abstraction
{
    public interface ICacheableQuery<TResponse> : IQuery<TResponse>
    {
        [JsonIgnore]
        public string? CacheKey => null;
        [JsonIgnore]
        public TimeSpan? Ttl => null;
        [JsonIgnore]
        public IEnumerable<string> Tags => Enumerable.Empty<string>();
    }
}