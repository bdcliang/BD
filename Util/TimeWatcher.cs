using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace BD.Util
{
    public class TimeWatcher
    {
        /// <summary>
        /// Fire a Time watcher
        /// </summary>
        /// <param name="body"></param>
        /// <param name="handler"></param>
        public static void Fire(Action body,Action<long> handler=null)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            body();
            sw.Stop();
            handler?.Invoke(sw.ElapsedMilliseconds);
        }
    }
}
