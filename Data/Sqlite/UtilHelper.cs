using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace BD.Data
{
    /// <summary>
    /// 
    /// </summary>
    public class UtilHelper
    {
        /// <summary>
        /// 调试程序输出时间
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bodyAction"></param>
        /// <param name="completedAction"></param>
        /// <param name="state"></param>
        public static void DebugWatch(Action<object> bodyAction,Action<long> completedAction,object state=null)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            bodyAction(state);
            sw.Stop();
            completedAction(sw.ElapsedMilliseconds);
        }
    }
}
