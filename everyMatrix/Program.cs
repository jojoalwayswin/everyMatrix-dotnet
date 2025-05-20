using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using everyMatrix.Controller;

namespace everyMatrix;

public class Program
{
  static HttpListener httpListener;
  //static string url="http://localhost:8080/";
  static string url = "http://*:8080/";
 
  public static void Main(string[] args)
  {
    try
    {
      Console.WriteLine("监听状态：正在启动");
      httpListener = new HttpListener();
      httpListener.Prefixes.Add(url);
      httpListener.Start();
      // 异步监听客户端请求
      httpListener.BeginGetContext(ListenerHandle, httpListener);
      Console.WriteLine("监听状态：已启动"); 
      Console.Read();
      
    }
    catch (Exception e)
    {
      Console.WriteLine("监听状态：失败");
      Console.WriteLine(e);
      throw;
    }
   
  }
  
  /// <summary>
/// 监听回调函数
/// </summary>
private static void ListenerHandle(IAsyncResult result)
{
　　try
　　　　{
          Console.WriteLine($"接到新的请求:{result},时间：{DateTime.Now.ToString()}");
　　　　　　if (httpListener.IsListening)
　　　　　　{
            httpListener.BeginGetContext(ListenerHandle, result);
　　　　　　　　HttpListenerContext context = httpListener.EndGetContext(result);
　　　　　　　　HttpListenerResponse response = context.Response;
　　　　　　　　HttpListenerRequest request = context.Request;
 
　　　　　　　　context.Response.AppendHeader("Access-Control-Allow-Origin", "*");//后台跨域请求，通常设置为配置文件
　　　　　　　　context.Response.AppendHeader("Access-Control-Allow-Credentials", "true"); //后台跨域请求
　　　　　　　　response.StatusCode = 200;
　　　　　　　　response.ContentType = "application/json;charset=UTF-8";
　　　　　　　　context.Response.AddHeader("Content-type", "text/plain");//添加响应头信息
　　　　　　　　context.Response.AddHeader("Content-type", "application/x-www-form-urlencoded");//添加响应头信息
 
　　　　　　　　response.ContentEncoding = Encoding.UTF8;
 
　　　　　　　　//解析Request请求
　　　　　　　　string content = "";


        string path = request.Url.LocalPath;
        Object resp = null;
        switch (request.HttpMethod)
        {
          case "POST":
            if (Regex.IsMatch(path, @"^\/customer\/(\d+)\/score\/(\d+)$"))
            {
              Match match = Regex.Match(path, @"^\/customer\/(\d+)\/score\/(\d+)$");
              int customerId = int.Parse(match.Groups[1].Value);
              int score = int.Parse(match.Groups[2].Value); 
              resp = RankController.GetInstance().UpdateScore(customerId, score);
            }
            break;
          case "GET":
            if (Regex.IsMatch(path, @"^\/leaderboard\?start=(\d+)&end=(\d+)$"))
            {
              Match match = Regex.Match(path, @"^\/leaderboard\?start=(\d+)&end=(\d+)$");
              int start = int.Parse(match.Groups[1].Value);
              int end = int.Parse(match.Groups[2].Value);
              resp = RankController.GetInstance().GetLeaderboard(start,end);
            }
            else if (Regex.IsMatch(path, @"^\/leaderboard\/(\d+)(\?.*)?high={\d+}&low={\d+}"))
            {
              Match match = Regex.Match(path, @"^\/leaderboard\/(\d+)(\?.*)?$");
              int customerId = int.Parse(match.Groups[1].Value);
              // 解析查询参数中的 high 和 low
              int high = int.Parse(match.Groups[2].Value);
              int low = int.Parse(match.Groups[3].Value);
              resp = RankController.GetInstance().GetCustomerById(customerId,  high, low);
            }
            break;
          default:
            Console.WriteLine("Unsupported HTTP Method");
            break;
        }
        
          Console.WriteLine(content);
　　　　　　string responseString = content;
　　　　　　byte[] buffers = System.Text.Encoding.UTF8.GetBytes(responseString);
　　　　　　
　　　　　　response.ContentLength64 = buffers.Length;
　　　　　　System.IO.Stream output = response.OutputStream;
　　　　　　output.Write(buffers, 0, buffers.Length);
　　　　　　// You must close the output stream.
　　　　　　output.Close();
 
　　　　}
 
　　}
　　catch (Exception ex)
　　{
    Console.WriteLine("ex.Message"); 
　　}
}

}