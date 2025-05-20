using everyMatrix.domain;

namespace everyMatrix.util;
/**
 * 自定义的跳表类
 */
public class CustomSkipList
{
    private const int MaxLevel = 16;
    private readonly Random _random = new();
    private SkipListNode _head = new(new RankModel(-1, 0), MaxLevel);
    private int _level;
    private int _count;

    public int Count => _count;
    
    private int RandomLevel()
    {
        int level = 1;
        while (_random.NextDouble() < 0.5 && level < MaxLevel)
            level++;
        return level;
    }
    public void Insert(RankModel customer)
    {
        var update = new SkipListNode[MaxLevel];
        var current = _head;

        for (int i = _level; i >= 0; i--)
        {
            while (current.Forward[i] != null && SkipListNode.Compare(current.Forward[i], new SkipListNode(customer, i)) < 0)
                current = current.Forward[i];
            update[i] = current;
        }

        current = current.Forward[0];

        if (current == null || SkipListNode.Compare(current, new SkipListNode(customer, 0)) != 0)
        {
            int nodeLevel = RandomLevel();
            if (nodeLevel > _level)
            {
                for (int i = _level + 1; i < nodeLevel; i++)
                    update[i] = _head;
                _level = nodeLevel;
            }

            var newNode = new SkipListNode(customer, nodeLevel);

            for (int i = 0; i < nodeLevel; i++)
            {
                newNode.Forward[i] = update[i].Forward[i];
                update[i].Forward[i] = newNode;
            }

            _count++;
        }
        
    }
    
    public bool Delete(RankModel customer)
    {
        var update = new SkipListNode[MaxLevel];
        var current = _head;

        for (int i = _level; i >= 0; i--)
        {
            while (current.Forward[i] != null && SkipListNode.Compare(current.Forward[i], new SkipListNode(customer, i)) < 0)
                current = current.Forward[i];
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
            }

            _count--;
            return true;
        }

        return false;
    }
    public RankModel GetByRank(int rank)
    {
        if (rank < 1 || rank > _count)
            throw new IndexOutOfRangeException("Rank out of range");

        var current = _head.Forward[0];
        for (int i = 1; i < rank && current != null; i++)
            current = current.Forward[0];

        return current?.RankModel;
    }
    /// <summary>
    /// 获取指定排名范围内的客户数组
    /// </summary>
    /// <param name="start">起始排名（包含）</param>
    /// <param name="end">结束排名（包含）</param>
    /// <returns>RankModel 数组</returns>
    public RankModel[] GetRangeByRank(int start, int end)
    {
        if (start < 1 || end > _count || start > end)
            return Array.Empty<RankModel>();

        int size = end - start + 1;
        RankModel[] result = new RankModel[size];

        // 定位到 start 的节点
        var current = _head.Forward[0];
        for (int i = 1; i < start && current != null; i++)
        {
            current = current.Forward[0];
        }

        for (int i = 0; i < size && current != null; i++)
        {
            result[i] = current.RankModel;
            current = current.Forward[0];
        }

        return result;
    }
}