using everyMatrix.domain;
using everyMatrix.util;

namespace everyMatrix.Controller;

public class RankController
{
    private static readonly RankController Instance = new RankController();
    private const int SegmentCount = 1;
    private readonly ReaderWriterLockSlim[] _segmentLocks = new ReaderWriterLockSlim[SegmentCount];
    private CustomHashMap<long,RankModel> _customers = new();
    private CustomSkipList _skipList = new();

    private ReaderWriterLockSlim GetWriteLockForUser(int customerId)
    {
        int index = Math.Abs(customerId) % SegmentCount;
        return _segmentLocks[index];
    }
    public RankController()
    {
        for (int i = 0; i < SegmentCount; i++)
        {
            _segmentLocks[i] = new ReaderWriterLockSlim();
        }
    }
    public static RankController GetInstance()
    {
        return Instance;
    }

    public int UpdateScore(int customerId, int newScore)
    {   
        var writerLock = GetWriteLockForUser(customerId);
        writerLock.EnterWriteLock();
        try
        {
            if (_customers.Get(customerId) is RankModel existing)
            {
                _skipList.Delete(existing);
                existing.Score += newScore;
            }
            else
            {
                existing = new RankModel(customerId, newScore);
            }
            if (existing.Score >0)
            {
                existing = _skipList.Insert(existing);
                _customers.Put(customerId, existing);
            }
            return existing.Score;
        }finally
        {
            writerLock.ExitWriteLock();
        }
    }

    public  RankModel[] GetLeaderboard(int start, int end)
    {
        int count = _skipList.Count;
        start = Math.Max(1, start);
        end = Math.Min(count, end);

        if (start > end)
            return Array.Empty<RankModel>();
        // 使用 GetRangeByRank 获取指定范围的客户
        RankModel[] result = _skipList.GetRangeByRank(start, end);
        // 遍历并设置 Rank（根据传入的 start 起始）
        for (int i = 0; i < result.Length; i++)
        {
            result[i].Rank = start + i;
        }
        return result;
    }

    public RankModel[] GetCustomerById(long customerId, int high, int low)
    {
        var customer = _customers.Get(customerId);
        if (customer == null || customer.Score <= 0)
            return Array.Empty<RankModel>();

        SkipListNode node = _skipList.GetSkipListNode(customer); 
        if (node == null)
            return Array.Empty<RankModel>();
        // 使用双向链表快速定位前后节点
        List<RankModel> result = new List<RankModel>();
        // 当前节点
        customer.Rank = node.RankModel.Rank; 
        result.Add(customer);
        // 前面的 high 个节点（向前遍历）
        SkipListNode prev = node.Backward[0];
        int prevCount = 0;
        while (prev != null && prevCount < high)
        {
            if (prev.RankModel.Score > 0)
            {
                result.Insert(0, prev.RankModel);
                prevCount++;
            }
            prev = prev.Backward[0];
        }

        // 后面的 low 个节点（向后遍历）
        SkipListNode next = node.Forward[0];
        int nextCount = 0;
        while (next != null && nextCount < low)
        {
            if (next.RankModel.Score > 0)
            {
                result.Add(next.RankModel);
                nextCount++;
            }
            next = next.Forward[0];
        }
        return result.ToArray();
    }

}