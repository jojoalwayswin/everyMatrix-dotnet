using everyMatrix.domain;
namespace everyMatrix.util;
/**
 * 自定义的跳表类
 */
public class CustomSkipList
{
    private const int MaxLevel = 16;
    private readonly Random _random = new();
    private SkipListNode _head;
    private SkipListNode _tail;
    private int _level;
    private int _count;
    private readonly ReaderWriterLockSlim _rwLock = new();
    public int Count => _count;
    private const int SegmentCount = 16;
    private readonly ReaderWriterLockSlim[] _segmentLocks = Enumerable.Range(0, SegmentCount)
        .Select(_ => new ReaderWriterLockSlim())
        .ToArray();
    private ReaderWriterLockSlim GetLockForUser(long id)
    {
        return _segmentLocks[Math.Abs(id) % SegmentCount];
    }
    public CustomSkipList()
    {
        // 初始化头节点，所有层级指向 null
        _head = new SkipListNode(new RankModel(-1, 0,0), MaxLevel);
        _tail = null;
    }
   
    private int RandomLevel()
    {
        int level = 1;
        while (_random.NextDouble() < 0.5 && level < MaxLevel)
            level++;
        return level;
    }

    public RankModel Insert(RankModel customer)
    {
        var writerLock = GetLockForUser(customer.CustomerId);
        writerLock.EnterWriteLock();
        try
        {
            var update = new SkipListNode[MaxLevel];
            var current = _head;
            // 从最高层向下查找每层的前驱节点
            for (int i = _level - 1; i >= 0; i--)
            {
                while (current.Forward[i] != null &&
                       SkipListNode.Compare(current.Forward[i], new SkipListNode(customer, i)) < 0)
                {
                    current = current.Forward[i];
                }
                update[i] = current;
            }

            current = current.Forward[0];
            // 如果节点不存在，则插入新节点
            if (current == null || SkipListNode.Compare(current, new SkipListNode(customer, 0)) != 0)
            {
                int nodeLevel = Math.Min(RandomLevel(), MaxLevel);
                if (nodeLevel > _level)
                {
                    for (int i = _level; i < nodeLevel; i++)
                        update[i] = _head;
                    _level = nodeLevel;
                }
                var newNode = new SkipListNode(customer, nodeLevel);
                // 插入各层
                for (int i = 0; i < nodeLevel; i++)
                {
                    newNode.Forward[i] = update[i].Forward[i];
                    update[i].Forward[i] = newNode;

                    // 更新后向指针
                    if (newNode.Forward[i] != null)
                        newNode.Forward[i].Backward[i] = newNode;
                    newNode.Backward[i] = update[i];
                }
                // 更新尾节点（level 0）
                if (newNode.Forward[0] == null)
                    _tail = newNode;
                _count++;
                return newNode.RankModel;
            }
            return null; // 已存在，未新增
        }
        finally
        {
            writerLock.ExitWriteLock();
        }
    }
    public SkipListNode GetSkipListNode(RankModel model)
    {
        _rwLock.EnterReadLock();
        try
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));
            var current = _head.Forward[0];
            int index = 1;
            while (current != null)
            {
                if (SkipListNode.Compare(current, new SkipListNode(model, 0)) == 0)
                {
                    return current;
                }
                current = current.Forward[0];
                index++;
            }
            return null; // 表示未找到
        }
        finally
        {
            _rwLock.ExitReadLock();
        }
    }
    public bool Delete(RankModel customer)
    {
        var writerLock = GetLockForUser(customer.CustomerId);
        writerLock.EnterWriteLock();
        try
        {
            var update = new SkipListNode[MaxLevel];
            var current = _head;
            for (int i = _level - 1; i >= 0; i--)
            {
                while (current.Forward[i] != null &&
                       SkipListNode.Compare(current.Forward[i], new SkipListNode(customer, i)) < 0)
                {
                    current = current.Forward[i];
                }
                update[i] = current;
            }
            current = current.Forward[0];
            if (current != null && current.RankModel.CustomerId == customer.CustomerId)
            {
                for (int i = 0; i < _level; i++)
                {
                    if (update[i].Forward[i] != current)
                        break;
                    update[i].Forward[i] = current.Forward[i];
                    // 更新后向指针
                    if (current.Forward[i] != null)
                        current.Forward[i].Backward[i] = update[i];
                }
                // 更新 tail
                if (_tail == current)
                    _tail = current.Backward[0];
                _count--;
                return true;
            }
            return false;
        }
        finally
        {
            writerLock.ExitWriteLock();
        }
    }
    
    public RankModel GetByRank(int rank)
    {
        _rwLock.EnterReadLock();
        try
        {
            if (rank < 1 || rank > _count)
                throw new IndexOutOfRangeException("Rank out of range");
            SkipListNode current = _head.Forward[0];
            // 如果接近尾部，则从后往前查找
            if (rank > _count / 2)
            {
                int steps = _count - rank + 1;
                current = _tail;
                for (int i = 1; i < steps && current != null; i++)
                {
                    current = current.Backward[0];
                }
            }
            else
            {
                for (int i = 1; i < rank && current != null; i++)
                {
                    current = current.Forward[0];
                }
            }
            return current?.RankModel;
        }
        finally
        {
            _rwLock.ExitReadLock();
        }
    }
    /// <summary>
    /// 获取指定排名范围内的客户数组
    /// </summary>
    /// <param name="start">起始排名（包含）</param>
    /// <param name="end">结束排名（包含）</param>
    /// <returns>RankModel 数组</returns>
    public RankModel[] GetRangeByRank(int start, int end)
    {
        _rwLock.EnterReadLock();
        try
        {
            if (start < 1 || end > _count || start > end)
                return Array.Empty<RankModel>();
            int size = end - start + 1;
            RankModel[] result = new RankModel[size];
            // 判断从哪一端更近：头部还是尾部
            SkipListNode current;
            if (start <= _count / 2)
            {
                // 从头部开始向前查找
                current = _head.Forward[0];
                for (int i = 1; i < start && current != null; i++)
                {
                    current = current.Forward[0];
                }
            }
            else
            {
                // 从尾部开始向后查找
                current = _tail;
                for (int i = _count; i > start && current != null; i--)
                {
                    current = current.Backward[0];
                }
            }
            // 一次性读取连续范围内的节点
            for (int i = 0; i < size && current != null; i++)
            {
                result[i] = current.RankModel;
                current = current.Forward[0];
            }
            return result;
        }
        finally
        {
            _rwLock.ExitReadLock();
        }
    }
    
}