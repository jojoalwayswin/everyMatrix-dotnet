using everyMatrix.domain;

namespace everyMatrix.util;

 public class CustomHashMap<TKey, TValue>
    {
        private class Entry
        {
            public TKey Key { get; set; }
            public TValue Value { get; set; }
            public Entry Next { get; set; }
        }

        private const int InitialCapacity = 16;
        private const float LoadFactor = 0.75f;
        private Entry[] _buckets;
        private int _count;
        private readonly float _loadFactor;
        private int _threshold;
        public int Count()
        {
            return _count;
        }
        public Boolean ContainsKey(TKey key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));

            int index = GetBucketIndex(key);
            Entry entry = _buckets[index];

            while (entry != null)
            {
                if (Equals(entry.Key, key))
                    return true;
                entry = entry.Next;
            }

            return false;
        }
        public CustomHashMap() : this(InitialCapacity, LoadFactor)
        {
        }

        public CustomHashMap(int initialCapacity) : this(initialCapacity, LoadFactor)
        {
        }
        public CustomHashMap(int initialCapacity, float loadFactor)
        {
            if (initialCapacity <= 0) throw new ArgumentException("Capacity must be greater than zero.");
            if (loadFactor <= 0) throw new ArgumentException("Load factor must be greater than zero.");

            _buckets = new Entry[initialCapacity];
            _count = 0;
            _loadFactor = loadFactor;
            _threshold = (int)(initialCapacity * loadFactor);
        }

        private int GetBucketIndex(TKey key)
        {
            // 使用默认的 GetHashCode，并确保非负数
            return Math.Abs(EqualityComparer<TKey>.Default.GetHashCode(key)) % _buckets.Length;
        }

        public void Put(TKey key, TValue value)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            // 检查是否需要扩容
            if (_count >= _threshold)
            {
                Resize();
            }   
            int index = GetBucketIndex(key);
            Entry entry = _buckets[index];

            while (entry != null)
            {
                if (Equals(entry.Key, key))
                {
                    entry.Value = value;
                    return;
                }
                entry = entry.Next;
            }

            var newEntry = new Entry { Key = key, Value = value, Next = _buckets[index] };
            _buckets[index] = newEntry;
            _count++;
        }

        public TValue Get(TKey key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));

            int index = GetBucketIndex(key);
            Entry entry = _buckets[index];

            while (entry != null)
            {
                if (Equals(entry.Key, key))
                    return entry.Value;
                entry = entry.Next;
            }

            return default(TValue);
        }
        private void Resize()
        {
            int newCapacity = _buckets.Length * 2;
            Entry[] oldBuckets = _buckets;

            // 创建新桶数组
            _buckets = new Entry[newCapacity];
            _threshold = (int)(newCapacity * _loadFactor);
            _count = 0;

            // 迁移原有数据
            foreach (var oldBucket in oldBuckets)
            {
                Entry entry = oldBucket;
                while (entry != null)
                {
                    Put(entry.Key, entry.Value); // 重新插入
                    entry = entry.Next;
                }
            }
        }
    }