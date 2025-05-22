using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using everyMatrix.Controller;

namespace everyMatrix.util;

public class HttpListenerHandler
{
    
  /// <summary>
/// 监听回调函数
/// </summary>
///

public static void ListenerHandle(IAsyncResult result)
{
  try
  {
    if (!Program.httpListener.IsListening) return;

    Program.httpListener.BeginGetContext(ListenerHandle, result);
    HttpListenerContext context = Program.httpListener.EndGetContext(result);

    // 处理请求
    ProcessRequest(context);
  }
  catch (Exception ex)
  {
    Console.WriteLine($"Error handling request: {ex.Message}");
   
  }
}
  private static void ProcessRequest(HttpListenerContext context)
  {
    var request = context.Request;
    var response = context.Response;
       object resp = null;
    int statusCode = 200;

    try
    {
      string fullUrl = request.Url.ToString();
      string authority = request.Url.Authority;
      int index = fullUrl.IndexOf(authority)+authority.Length;
      int length = fullUrl.Length - index;
      string path =fullUrl.Substring(fullUrl.IndexOf(authority)+authority.Length, length);

      switch (request.HttpMethod)
      {
        case "POST":
          resp = HandlePostRequest(path, out statusCode);
          break;
        case "GET":
          resp = HandleGetRequest(path, out statusCode);
          break;
        default:
          statusCode = 400;
          resp = new { error = "Unsupported HTTP Method" };
          break;
      }
    }
    catch (Exception ex)
    {
      statusCode = 500;
      resp = new { error = "Internal Server Error", message = ex.Message };
      Console.WriteLine("Unhandled exception:");
      Console.WriteLine(ex);
    }

    WriteResponse(response, resp, statusCode);
  }
  private static object HandleGetRequest(string path, out int statusCode)
  {
    if (Regex.IsMatch(path, @"^\/leaderboard\?(.*)$"))
    {
      Match match = Regex.Match(path, @"^\/leaderboard\?(.*)$");
      string queryString = match.Groups[1].Value;
      var queryParams = ParseQueryString(queryString);

      if (!int.TryParse(queryParams.Get("start"), out int start) ||
          !int.TryParse(queryParams.Get("end"), out int end))
      {
        statusCode = 400;
        return new { error = "start and end must be valid integers" };
      }

      statusCode = 200;
      return RankController.GetInstance().GetLeaderboard(start, end);
    }
    else if (Regex.IsMatch(path, @"^\/leaderboard\/(\d+)\?(.*)$"))
    {
      Match match = Regex.Match(path, @"^\/leaderboard\/(\d+)\?(.*)$");
      int customerId = int.Parse(match.Groups[1].Value);

      string queryString = match.Groups[2].Value;
      var queryParams = ParseQueryString(queryString);

      if (!int.TryParse(queryParams.Get("high"), out int high) ||
          !int.TryParse(queryParams.Get("low"), out int low))
      {
        statusCode = 400;
        return new { error = "high and low must be valid integers" };
      }

      statusCode = 200;
      return RankController.GetInstance().GetCustomerById(customerId, high, low);
    }
    else
    {
      statusCode = 404;
      return new { error = "Not Found" };
    }
  }
  private static object HandlePostRequest(string path, out int statusCode)
  {
    if (Regex.IsMatch(path, @"^\/customer\/(\d+)\/score\/(-?\d+)$"))
    {
      Match match = Regex.Match(path, @"^\/customer\/(\d+)\/score\/(-?\d+)$");
      int customerId = int.Parse(match.Groups[1].Value);
      int score = int.Parse(match.Groups[2].Value);

      statusCode = 200;
      return RankController.GetInstance().UpdateScore(customerId, score);
    }
    else
    {
      statusCode = 404;
      return new { error = "Not Found" };
    }
  }
  private static void WriteResponse(HttpListenerResponse response, object content, int statusCode)
  {
    response.StatusCode = statusCode;
    response.ContentType = "application/json;charset=UTF-8";
    response.ContentEncoding = Encoding.UTF8;
    response.AppendHeader("Access-Control-Allow-Origin", "*");
    response.AppendHeader("Access-Control-Allow-Credentials", "true");

    string jsonContent = JsonSerializer.Serialize(content) ;
    byte[] buffer = Encoding.UTF8.GetBytes(jsonContent);

    response.ContentLength64 = buffer.Length;
    using (Stream output = response.OutputStream)
    {
      output.Write(buffer, 0, buffer.Length);
    }
  }


  static CustomHashMap<string, string> ParseQueryString(string queryString)
  {
    var result = new CustomHashMap<string, string>();

    if (string.IsNullOrEmpty(queryString))
      return result;

    // 分割每个键值对
    string[] pairs = queryString.Split('&');

    foreach (string pair in pairs)
    {
      if (string.IsNullOrEmpty(pair))
        continue;

      string[] parts = pair.Split('=');
      if (parts.Length == 2)
      {
        string key = parts[0];
        string value = parts[1];
        if (key == "")
        {
          continue;
        }
        
        result.Put(key,value);
      }
    }

    return result;
  }
}