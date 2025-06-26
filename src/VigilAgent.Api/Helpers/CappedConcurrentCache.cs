    using System.Collections.Concurrent;
    using VigilAgent.Api.Commons;

namespace VigilAgent.Api.Helpers{

    public class CappedConcurrentCache<T> where T : ITelemetryItem
    {
        private readonly int _maxSize;
        private readonly ConcurrentQueue<string> _order = new();
        private readonly ConcurrentDictionary<string, T> _cache = new();

        public CappedConcurrentCache(int maxSize)
        {
            _maxSize = maxSize;
        }

        public void Add(T item)
        {
            _cache[item.Id] = item;
            _order.Enqueue(item.Id);

            while (_cache.Count > _maxSize && _order.TryDequeue(out var oldKey))
            {
                _cache.TryRemove(oldKey, out _);
            }
        }

        public IEnumerable<T> GetAll(Func<T, bool> predicate = null)
        {
            return predicate == null ? _cache.Values : _cache.Values.Where(predicate);
        }

        public bool IsEmpty => _cache.IsEmpty;

        public void Clear()
        {
            _cache.Clear();
        }
    }


}
