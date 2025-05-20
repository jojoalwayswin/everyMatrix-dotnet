using everyMatrix.domain;

namespace everyMatrix.util;

public class CustomHashMap
{
    private class Entry
    {
        public long Key { get; set; }
        public RankModel Value { get; set; }
        public Entry Next { get; set; }
    }
    private const int InitialCapacity = 16;
    private Entry[] _buckets;
    private int _count;
    public CustomHashMap()
    {
        _buckets = new Entry[InitialCapacity];
        _count = 0;
    }
    private int GetBucketIndex(long key)
    {
        return Math.Abs(key.GetHashCode()) % _buckets.Length;
    }
    public void Put(long key, RankModel value)
    {
        int index = GetBucketIndex(key);
        Entry entry = _buckets[index];

        while (entry != null)
        {
            if (entry.Key == key)
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
    public RankModel Get(long key)
    {
        int index = GetBucketIndex(key);
        Entry entry = _buckets[index];

        while (entry != null)
        {
            if (entry.Key == key)
                return entry.Value;
            entry = entry.Next;
        }

        return null;
    }
    public bool Remove(long key)
    {
        int index = GetBucketIndex(key);
        Entry prev = null;
        Entry current = _buckets[index];

        while (current != null)
        {
            if (current.Key == key)
            {
                if (prev == null)
                    _buckets[index] = current.Next;
                else
                    prev.Next = current.Next;

                _count--;
                return true;
            }

            prev = current;
            current = current.Next;
        }

        return false;
    }
}