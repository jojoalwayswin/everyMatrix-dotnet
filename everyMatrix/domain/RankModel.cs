namespace everyMatrix.domain;

public class RankModel
{
    public long CustomerId { get; set; }
    public int Score { get; set; }
    public int Rank { get; set; }
    
    public RankModel(long customerId, int score)
    {
        CustomerId = customerId;
        Score = score;
    }
    public RankModel(long customerId, int score , int rank)
    {
        CustomerId = customerId;
        Score = score;
        Rank = rank;
    }
}