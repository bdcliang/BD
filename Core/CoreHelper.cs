using System;
using System.Collections.Generic;
using System.Text;

namespace BD
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class CoreHelper
    {
        /// <summary>
        /// Relay Execute Function
        /// </summary>
        /// <param name="time">delay time</param>
        /// <param name="action"></param>
        public void DelayFun(double time, Action<object> action = null)
        {
            System.Timers.Timer timer = new System.Timers.Timer(time);
            timer.AutoReset = false;
            timer.Elapsed += delegate {
                action?.Invoke(1);
                timer.Dispose();
            };
            timer.Start();
        }
    }
}
