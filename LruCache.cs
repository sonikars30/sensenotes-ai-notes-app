namespace SenseNotes.Api;

/// <summary>O(1) get/put: dictionary for lookup + doubly linked list for recency order.</summary>
public class LruCache<K, V> where K : notnull
{
    private readonly int _capacity;
    private readonly Dictionary<K, LinkedListNode<(K Key, V Value)>> _map = new();
    private readonly LinkedList<(K Key, V Value)> _order = new();
    private readonly object _lock = new();

    public LruCache(int capacity) => _capacity = capacity;

    public V? Get(K key)
    {
        lock (_lock)
        {
            if (!_map.TryGetValue(key, out var node)) return default;
            _order.Remove(node);
            _order.AddFirst(node);          // most recently used goes to the front
            return node.Value.Value;
        }
    }

    public void Put(K key, V value)
    {
        lock (_lock)
        {
            if (_map.TryGetValue(key, out var existing)) { _order.Remove(existing); _map.Remove(key); }
            else if (_map.Count >= _capacity)
            {
                var last = _order.Last!;     // least recently used is evicted
                _order.RemoveLast();
                _map.Remove(last.Value.Key);
            }
            var node = new LinkedListNode<(K Key, V Value)>((key, value));
            _order.AddFirst(node);
            _map[key] = node;
        }
    }
}
