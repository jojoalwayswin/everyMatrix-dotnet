namespace everyMatrix.domain;

public class SkipListNode
{
    public RankModel RankModel { get; set; }
    public SkipListNode[] Forward;
    public SkipListNode[] Backward;

    public SkipListNode(RankModel rankModel, int level)
    {
        RankModel = rankModel;
        Forward = new SkipListNode[level];
        Backward = new SkipListNode[level];
    }

    public static int Compare(SkipListNode a, SkipListNode b)
    {
        if (a.RankModel.Score != b.RankModel.Score)
            return b.RankModel.Score.CompareTo(a.RankModel.Score); // 降序
        return a.RankModel.CustomerId.CompareTo(b.RankModel.CustomerId); // ID升序
    }
}