using everyMatrix.Controller;

namespace everyMatrix_dotnet_test;

public class Tests
{
    private readonly RankController _rankController;

    public Tests()
    {
        // 初始化被测类
        _rankController = new RankController();
    }

    [Test]
    public void UpdateScore_WithValidInput_ReturnsZero()
    {
        // Arrange
        int customerId = 123;
        int score = 90;

        // Act
        int result = _rankController.UpdateScore(customerId, score);

        // Assert
        Assert.Equals(0, result);
    }

    [Test]
    public void UpdateScore_WithInvalidCustomerId_ReturnsZero()
    {
        // Arrange
        int customerId = -1;  // 非法ID
        int score = 90;

        // Act
        int result = _rankController.UpdateScore(customerId, score);

        // Assert
        Assert.Equals(0, result);
    }

    [Test]
    public void UpdateScore_WithInvalidScore_ReturnsZero()
    {
        // Arrange
        int customerId = 123;
        int score = -5;  // 非法分数

        // Act
        int result = _rankController.UpdateScore(customerId, score);

        // Assert
        Assert.Equals(0, result);
    }
   
}