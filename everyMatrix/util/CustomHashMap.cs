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
        private readonly ReaderWriterLockSlim _rwLock = new();
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
            {
                if (key == null) return 0;
                // 使用默认 GetHashCode 并确保非负数
                int hash;
                if (typeof(TKey) == typeof(string))
                {
                    // 对 string 做额外稳定性处理（可选）
                    hash = StringToStableHash(key.ToString());
                }
                else
                {
                    hash = key.GetHashCode();
                }
                // 保证正数
                return Math.Abs(hash) % _buckets.Length;
            }
        }
        // 可选：对 string 实现稳定哈希
        private static int StringToStableHash(string s)
        {
            unchecked
            {
                int hash = 17;
                foreach (char c in s)
                {
                    hash = hash * 23 + c;
                }
                return hash;
            }
        }
        public void Put(TKey key, TValue value)
        {
            _rwLock.EnterWriteLock();
            try
            {
                InsertInternal(key, value);
            }
            finally
            {
                _rwLock.ExitWriteLock();
            }
        }
        private void InsertInternal(TKey key, TValue value)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            // 检查是否需要扩容（Resize 时不再触发）
            if (_count >= _threshold)
            {
                ResizeInternal(); // 不再调用 Put
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
            _rwLock.EnterReadLock();
            try
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
            finally
            {
                _rwLock.ExitReadLock();
            }
        }
        private void ResizeInternal()
        {
            int newCapacity = _buckets.Length * 2;
            Entry[] oldBuckets = _buckets;
            _buckets = new Entry[newCapacity];
            _threshold = (int)(newCapacity * _loadFactor);
            _count = 0;
            foreach (var oldBucket in oldBuckets)
            {
                Entry entry = oldBucket;
                while (entry != null)
                {
                    InsertInternal(entry.Key, entry.Value); // 使用无锁插入
                    entry = entry.Next;
                }
            }
        }
    }