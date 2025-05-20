using everyMatrix.domain;
using everyMatrix.util;

namespace everyMatrix.Controller;

public class RankController
{
    private static readonly RankController Instance = new RankController();
    public static RankController GetInstance()
    {
        return Instance;
    }
    private CustomHashMap _customers = new();
    private CustomSkipList _skipList = new();
    public int UpdateScore(int customerId, int newScore)
    {
        if (_customers.Get(customerId) is RankModel existing)
        {

            if (existing.Score > 0)
                _skipList.Delete(existing);

            existing.Score += newScore;
            _customers.Put(customerId, existing);
        }
        else
        {
            existing = new RankModel(customerId, newScore);
            _customers.Put(customerId, existing);
        }

        if (newScore > 0)
            _skipList.Insert(existing);
        return existing.Score;
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

    public RankModel[] GetCustomerById(int customerId, int high, int low)
    {
        var customer = _customers.Get(customerId);
        if (customer == null || customer.Score <= 0)
            return Array.Empty<RankModel>();

        int rank = FindRankByCustomerId(customerId);
        if (rank == -1)
            return Array.Empty<RankModel>();

        customer.Rank = rank;

        int prevStart = Math.Max(1, rank - high);
        int prevEnd = rank - 1;
        int nextStart = rank + 1;
        int nextEnd = Math.Min(_skipList.Count, rank + low);

        int totalSize = (prevEnd - prevStart + 1) + 1 + (nextEnd - nextStart + 1);
        RankModel[] result = new RankModel[totalSize];

        int index = 0;

        // 前面的客户
        if (prevStart <= prevEnd)
        {
            var prevList = _skipList.GetRangeByRank(prevStart, prevEnd);
            Array.Copy(prevList, 0, result, index, prevList.Length);
            index += prevList.Length;
        }

        // 当前客户
        result[index++] = customer;

        // 后面的客户
        if (nextStart <= nextEnd)
        {
            var nextList = _skipList.GetRangeByRank(nextStart, nextEnd);
            Array.Copy(nextList, 0, result, index, nextList.Length);
        }

        return result;
    }

    // 辅助方法：根据 CustomerId 查找排名
    private int FindRankByCustomerId(long id)
    {
        for (int i = 1; i <= _skipList.Count; i++)
        {
            var current = _skipList.GetByRank(i);
            if (current.CustomerId == id)
                return i;
        }
        return -1;
    }
}