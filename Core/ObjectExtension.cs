using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
public static class ObjectExtension
{
    /// <summary>
    /// 时间测试
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="action"></param>
    /// <param name="continueWith"></param>
    public static void MeasureTime(this object obj,Action action,Action<long> continueWith=null)
    {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            action?.Invoke();
            sw.Stop();
            var all= sw.ElapsedMilliseconds;
            if(continueWith==null)
                Console.WriteLine(string.Format("===total:{0}ms  {1}===", all,DateTime.Now));
            else
              continueWith(all);
    }

    /// <summary>
    /// 日志记录
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="args"></param>
    [Conditional("DEBUG")]
    public static void log(this object obj,string format, params object[] args)
    {
        StringBuilder stringBuilder = new StringBuilder("[" + obj.GetType().ToString() + "]:[" + DateTime.Now + "]:");
        string mformat = string.Format("{0}{1}",stringBuilder.ToString(),format);
        Console.WriteLine(mformat,args);
    }
    /// <summary>
    /// Log 是否写入到本地
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="writeLocal"></param>
    /// <param name="args"></param>
    public static void log(this object obj, bool writeLocal,string format,params object[] args)
    {
        StringBuilder stringBuilder = new StringBuilder("[" + obj.GetType().ToString() + "]:[" + DateTime.Now + "]:");
        string mformat = string.Format("{0}{1}", stringBuilder.ToString(), format);
        var msg = string.Format(mformat,args);
        if (writeLocal)
        {
            string path = Environment.CurrentDirectory + @"\log";
            var stream = File.AppendText(path);
            stream.Write(msg);
            stream.WriteLine();
            stream.Flush();
            stream.Close();
        }
        else
            Console.WriteLine(msg);
    }

    public static void log(this object obj,string filepath, string format, params object[] args)
    {
        StringBuilder stringBuilder = new StringBuilder("[" + obj.GetType().ToString() + "]:[" + DateTime.Now + "]:");
        string mformat = string.Format("{0}{1}", stringBuilder.ToString(), format);
        var msg = string.Format(mformat, args);
        string path = filepath;
        var stream = File.AppendText(path);
        stream.Write(msg);
        stream.WriteLine();
        stream.Flush();
        stream.Close();
    }
}
