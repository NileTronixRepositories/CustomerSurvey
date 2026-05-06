using System.Collections;
using System.Collections.Concurrent;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace BuildingBlock.Application.Abstraction.Caching
{
    public static class CacheKeyFactory
    {
        private static readonly ConcurrentDictionary<Type, PropertyInfo[]> _propsCache = new();

        public static string BuildFromRequest(object request)
        {
            var t = request.GetType();

            var props = _propsCache.GetOrAdd(t, type =>
                type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    .Where(p => p.CanRead)
                    .OrderBy(p => p.Name)
                    .ToArray());

            var sb = new StringBuilder();
            sb.Append(t.FullName ?? t.Name);

            foreach (var p in props)
            {
                var v = p.GetValue(request);
                sb.Append('|').Append(p.Name).Append('=').Append(SerializeStable(v));
            }

            return sb.ToString();
        }

        private static string SerializeStable(object? v)
        {
            if (v is null) return "null";

            return v switch
            {
                string s => s,
                Guid g => g.ToString("N"),

                DateTime dt => dt.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture),
                DateTimeOffset dto => dto.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture),

                decimal d => d.ToString(CultureInfo.InvariantCulture),
                double db => db.ToString("R", CultureInfo.InvariantCulture),
                float f => f.ToString("R", CultureInfo.InvariantCulture),

                IDictionary dict => SerializeDictionary(dict),

                // IMPORTANT: handles List<Guid>, Guid[], IEnumerable<int> ... إلخ
                IEnumerable en => "[" + string.Join(",", en.Cast<object?>().Select(SerializeStable)) + "]",

                IFormattable fmt => fmt.ToString(null, CultureInfo.InvariantCulture) ?? v.ToString()!,
                _ => v.ToString() ?? v.GetType().Name
            };
        }

        private static string SerializeDictionary(IDictionary dict)
        {
            var items = new List<(string k, string v)>();

            foreach (DictionaryEntry entry in dict)
            {
                var k = SerializeStable(entry.Key);
                var v = SerializeStable(entry.Value);
                items.Add((k, v));
            }

            // ensure stable ordering
            items.Sort((a, b) => string.CompareOrdinal(a.k, b.k));

            return "{" + string.Join(",", items.Select(x => $"{x.k}:{x.v}")) + "}";
        }
    }
}