
using everyMatrix.Controller;
using NUnit.Framework.Legacy;

namespace everyMatrix_dotnet_test;
[TestFixture]
public class RankControllerTest
{
     private RankController _controller;

        [SetUp]
        public void Setup()
        {
            // 使用新的实例，避免其他测试干扰
            _controller = RankController.GetInstance();
        }

        [Test]
        public void TC01_UpdateScore_NewUser_PositiveScore_AddedToMapAndSkipList()
        {
            _controller = new RankController();
            // Arrange
            int customerId = 1;
            int score = 100;

            // Act
            int result = _controller.UpdateScore(customerId, score);

            // Assert
            ClassicAssert.AreEqual(score, result);

            var model = _controller.GetCustomerById(customerId, 0, 0);
            ClassicAssert.IsNotEmpty(model);
            ClassicAssert.AreEqual(customerId, model[0].CustomerId);
            ClassicAssert.AreEqual(score, model[0].Score);
        }

        [Test]
        public void TC02_UpdateScore_ExistingUser_IncreaseScore()
        {
            _controller = new RankController();
            // Arrange
            int customerId = 2;
            int initialScore = 100;
            int addedScore = 50;

            _controller.UpdateScore(customerId, initialScore);

            // Act
            int newScore = _controller.UpdateScore(customerId, addedScore);

            // Assert
            ClassicAssert.AreEqual(initialScore + addedScore, newScore);

            var model = _controller.GetCustomerById(customerId, 0, 0);
            ClassicAssert.AreEqual(initialScore + addedScore, model[0].Score);
        }

        [Test]
        public void TC03_GetLeaderboard_ValidRange_ReturnsExpectedResult()
        {
            _controller = new RankController();
            // Arrange
            _controller.UpdateScore(1, 100);
            _controller.UpdateScore(2, 200);
            _controller.UpdateScore(3, 150);

            // Act
            var result = _controller.GetLeaderboard(1, 3);

            // Assert
            ClassicAssert.AreEqual(3, result.Length);
            ClassicAssert.AreEqual(2, result[0].CustomerId); // 最高分
            ClassicAssert.AreEqual(3, result[1].CustomerId);
            ClassicAssert.AreEqual(1, result[2].CustomerId);
        }
        
        [Test]
        public void TC12_GetLeaderboard_WithSameScore_SortedByCustomerId()
        {
            _controller = new RankController();
            // Arrange - 添加相同分数的不同用户
            _controller.UpdateScore(3, 100); // ID: 3
            _controller.UpdateScore(1, 100); // ID: 1
            _controller.UpdateScore(2, 100); // ID: 2

            // Act
            var result = _controller.GetLeaderboard(1, 3);

            // Assert
            ClassicAssert.AreEqual(3, result.Length);
            ClassicAssert.AreEqual(1, result[0].CustomerId); // 按 ID 排序
            ClassicAssert.AreEqual(2, result[1].CustomerId);
            ClassicAssert.AreEqual(3, result[2].CustomerId);
        }
        
        [Test]
        public void TC04_GetLeaderboard_InvalidRange_ReturnsEmpty()
        {
            _controller = new RankController();
            // Arrange
            _controller.UpdateScore(1, 100);

            // Act
            var result = _controller.GetLeaderboard(5, 2);

            // Assert
            ClassicAssert.IsEmpty(result);
        }

        [Test]
        public void TC05_GetCustomerById_CustomerExists_ReturnsSurroundingPlayers()
        {
            _controller = new RankController();
            // Arrange
            _controller.UpdateScore(1, 100);
            _controller.UpdateScore(2, 200);
            _controller.UpdateScore(3, 150);
            _controller.UpdateScore(4, 50);

            // Act
            var result = _controller.GetCustomerById(3, 1, 1);

            // Assert
            ClassicAssert.AreEqual(3, result.Length);
            ClassicAssert.AreEqual(2, result[0].CustomerId); // 第一名
            ClassicAssert.AreEqual(3, result[1].CustomerId); // 当前用户
            ClassicAssert.AreEqual(1, result[2].CustomerId); // 第三名
        }

        [Test]
        public void TC06_GetCustomerById_CustomerDoesNotExist_ReturnsEmpty()
        {
            _controller = new RankController();
            // Act
            var result = _controller.GetCustomerById(999, 1, 1);

            // Assert
            ClassicAssert.IsEmpty(result);
        }

        [Test]
        public void TC07_GetCustomerById_HighLowZero_ReturnsOnlySelf()
        {
            _controller = new RankController();
            // Arrange
            _controller.UpdateScore(1, 100);
            _controller.UpdateScore(2, 200);
            _controller.UpdateScore(3, 150);

            // Act
            var result = _controller.GetCustomerById(2, 0, 0);

            // Assert
            ClassicAssert.AreEqual(1, result.Length);
            ClassicAssert.AreEqual(2, result[0].CustomerId);
        }
    [Test]
public void TC08_UpdateScore_UserGetsNegativeScore_RemovedFromSkipList()
{
    _controller = new RankController();
    // Arrange
    int customerId = 5;
    int initialScore = 100;

    _controller.UpdateScore(customerId, initialScore); // 先添加
    var beforeUpdate = _controller.GetLeaderboard(1, 10);
    ClassicAssert.IsTrue(Array.Exists(beforeUpdate, m => m.CustomerId == customerId));

    // Act
    int newScore = _controller.UpdateScore(customerId, -150);

    // Assert
    ClassicAssert.AreEqual(-50, newScore);

    var model = _controller.GetCustomerById(customerId, 0, 0);
    ClassicAssert.IsEmpty(model);

    var afterUpdate = _controller.GetLeaderboard(1, 10);
    ClassicAssert.IsFalse(Array.Exists(afterUpdate, m => m.CustomerId == customerId));
}

[Test]
public void TC09_UpdateScore_UserGetsZeroScore_RemovedFromSkipList()
{
    _controller = new RankController();
    // Arrange
    int customerId = 6;
    int initialScore = 200;

    _controller.UpdateScore(customerId, initialScore); // 先添加
    var beforeUpdate = _controller.GetLeaderboard(1, 10);
    ClassicAssert.IsTrue(Array.Exists(beforeUpdate, m => m.CustomerId == customerId));

    // Act
    int newScore = _controller.UpdateScore(customerId, -200);

    // Assert
    ClassicAssert.AreEqual(0, newScore);

    var model = _controller.GetCustomerById(customerId, 0, 0);
    ClassicAssert.IsEmpty(model);

    var afterUpdate = _controller.GetLeaderboard(1, 10);
    ClassicAssert.IsFalse(Array.Exists(afterUpdate, m => m.CustomerId == customerId));
}

[Test]
public void TC10_GetCustomerById_CustomerHasNegativeScore_ReturnsEmpty()
{
    _controller = new RankController();
    // Arrange
    int customerId = 7;
    _controller.UpdateScore(customerId, 100); // 添加
    _controller.UpdateScore(customerId, -200); // 变成负分

    // Act
    var result = _controller.GetCustomerById(customerId, 1, 1);

    // Assert
    ClassicAssert.IsEmpty(result);
}

[Test]
public void TC11_GetLeaderboard_AfterNegativeScore_UserNotInList()
{
    _controller = new RankController();
    // Arrange
    int customerId = 8;
    _controller.UpdateScore(customerId, 100); // 添加
    var before = _controller.GetLeaderboard(1, 10);
    ClassicAssert.IsTrue(Array.Exists(before, m => m.CustomerId == customerId));

    // Act
    _controller.UpdateScore(customerId, -100); // 得分为 0
    var after = _controller.GetLeaderboard(1, 10);

    // Assert
    ClassicAssert.IsFalse(Array.Exists(after, m => m.CustomerId == customerId));
}

}