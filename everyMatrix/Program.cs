using System.Net;
using everyMatrix.util;

namespace everyMatrix;

public class Program
{
  public static HttpListener httpListener;

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
      httpListener.BeginGetContext(HttpListenerHandler.ListenerHandle, httpListener);
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

}