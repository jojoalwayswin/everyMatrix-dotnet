
using System.Reflection;
using everyMatrix.util;
using NUnit.Framework.Legacy;

namespace everyMatrix_dotnet_test;

[TestFixture]
public class HttpListenerHandlerTest
{
    private MethodInfo _handleGetRequestMethod;

    private MethodInfo _handlePostRequestMethod;
    
    private MethodInfo _parseQueryStringMethod;
    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        
        _handleGetRequestMethod = typeof(HttpListenerHandler)
            .GetMethod("HandleGetRequest", BindingFlags.NonPublic | BindingFlags.Static);
        _handlePostRequestMethod = typeof(HttpListenerHandler)
            .GetMethod("HandlePostRequest", BindingFlags.NonPublic | BindingFlags.Static);
       
        _parseQueryStringMethod = typeof(HttpListenerHandler)
            .GetMethod("ParseQueryString", BindingFlags.Static | BindingFlags.NonPublic);

        ClassicAssert.NotNull(_handleGetRequestMethod);
        ClassicAssert.NotNull(_handlePostRequestMethod);
        ClassicAssert.NotNull(_parseQueryStringMethod);
        
    }
    [Test]
    [TestCase("/leaderboard?start=5&end=10")]
    public void TC01_HandleValidLeaderboardRequest_Returns200(string path)
    {
        var parameters = new object[] { path, 0 };
        var result = _handleGetRequestMethod.Invoke(null, parameters);

        int statusCode = (int)parameters[1];
        ClassicAssert.AreEqual(200, statusCode);
        
    }
    [Test]
    [TestCase("/leaderboard?start=abc&end=10")]
    public void TC02_HandleInvalidStartOrEnd_Returns400(string path)
    {
        var parameters = new object[] { path, 0 };
        var result = _handleGetRequestMethod.Invoke(null, parameters);

        int statusCode = (int)parameters[1];
        ClassicAssert.AreEqual(400, statusCode);
        
    }
    [Test]
    [TestCase("/leaderboard/123?high=abc&low=50")]
    public void TC04_HandleInvalidHighOrLow_Returns400(string path)
    {
        var parameters = new object[] { path, 0 };
        var result = _handleGetRequestMethod.Invoke(null, parameters);

        int statusCode = (int)parameters[1];
        ClassicAssert.AreEqual(400, statusCode);
    }

    [Test]
    [TestCase("/invalid-path")]
    public void TC05_HandleInvalidPath_Returns404(string path)
    {
        var parameters = new object[] { path, 0 };
        var result = _handleGetRequestMethod.Invoke(null, parameters);

        int statusCode = (int)parameters[1];
        ClassicAssert.AreEqual(404, statusCode);
        
    }
    [Test]
    [TestCase("/customer/123/score/456")]
    public void TC01_HandleValidCustomerScorePath_Returns200(string path)
    {
        
        var parameters = new object[] { path, 0 };

        // Act
        var result = _handlePostRequestMethod.Invoke(null, parameters);

        int statusCode = (int)parameters[1];
        // Assert
        ClassicAssert.AreEqual(200, statusCode);
    }

    [Test]
    [TestCase("/customer/abc/score/def")]
    public void TC02_HandleInvalidCustomerIdOrScore_Returns404(string path)
    {
        // Arrange
        var parameters = new object[] { path, 0 };
        // Act
        var result = _handlePostRequestMethod.Invoke(null, parameters);
        int statusCode = (int)parameters[1];

        // Assert
        ClassicAssert.AreEqual(404, statusCode);
    }

    [Test]
    [TestCase("/invalid-path")]
    public void TC03_HandleInvalidPath_Returns404(string path)
    {
        // Arrange
        var parameters = new object[] { path, 0 };

        // Act
        var result = _handlePostRequestMethod.Invoke(null, parameters);
        int statusCode = (int)parameters[1];

        // Assert
        ClassicAssert.AreEqual(404, statusCode);
        StringAssert.Contains("Not Found", result?.ToString());
    }
    [Test]
    [TestCase("")]
    public void TC01_Parse_EmptyOrNullString_ReturnsEmptyMap(string input)
    {
        var result = (CustomHashMap<string, string>)_parseQueryStringMethod.Invoke(null, new object[] { input });

        ClassicAssert.IsNotNull(result);
        ClassicAssert.AreEqual(0, result.Count());
    }
    
    [Test]
    [TestCase("key=value", "key", "value")]
    [TestCase("name=JohnDoe", "name", "JohnDoe")]
    public void TC02_Parse_SingleKeyValuePair_ReturnsCorrectValue(string input, string expectedKey, string expectedValue)
    {
        var result = (CustomHashMap<string, string>)_parseQueryStringMethod.Invoke(null, new object[] { input });

        ClassicAssert.IsTrue(result.ContainsKey(expectedKey));
        ClassicAssert.AreEqual(expectedValue, result.Get(expectedKey));
    }
    [Test]
    [TestCase("key1=value1&key2=value2", "key1", "value1", "key2", "value2")]
    [TestCase("a=1&b=2&c=3", "a", "1", "b", "2")]
    public void TC03_Parse_MultiplePairs_ReturnsAllValues(
        string input,
        string key1, string value1,
        string key2, string value2)
    {
        var result = (CustomHashMap<string, string>)_parseQueryStringMethod.Invoke(null, new object[] { input });

        ClassicAssert.IsTrue(result.ContainsKey(key1));
        ClassicAssert.AreEqual(value1, result.Get(key1));

        ClassicAssert.IsTrue(result.ContainsKey(key2));
        ClassicAssert.AreEqual(value2, result.Get(key2));
    }

    [Test]
    [TestCase("key=")]
    public void TC04_Parse_KeyWithEmptyValue_ReturnsEmptyValue(string input)
    {
        var result = (CustomHashMap<string, string>)_parseQueryStringMethod.Invoke(null, new object[] { input });

        var key = input.Split('=')[0];
        ClassicAssert.IsTrue(result.ContainsKey(key));
        ClassicAssert.AreEqual("", result.Get(key));
    }

    [Test]
    [TestCase("=value")]
    public void TC05_Parse_InvalidPair_StartsWithEqual_IgnoresEntry(string input)
    {
        var result = (CustomHashMap<string, string>)_parseQueryStringMethod.Invoke(null, new object[] { input });

        ClassicAssert.AreEqual(0, result.Count());
    }

    [Test]
    [TestCase("key1=value1&key2")]
    public void TC06_Parse_InvalidAndValidPairs_IgnoresInvalid(string input)
    {
        var result = (CustomHashMap<string, string>)_parseQueryStringMethod.Invoke(null, new object[] { input });

        ClassicAssert.IsTrue(result.ContainsKey("key1"));
        ClassicAssert.AreEqual("value1", result.Get("key1"));

        // key2 没有值，不加入 map
        ClassicAssert.IsFalse(result.ContainsKey("key2"));
    }

}