using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
namespace Borui.WinAPI
{
    /// <summary>
    /// 
    /// </summary>
    public class GCHelper
    {
        [DllImport("kernel32.dll")]
        private static extern bool SetProcessWorkingSetSize(IntPtr handle, int minimumWorkingSetSize, int maximumWorkingSetSize);
        /// <summary>
        /// 回收内存，将内存占用设为最小
        /// </summary>
        public static void ClearWorkSize()
        {
            SetProcessWorkingSetSize(System.Diagnostics.Process.GetCurrentProcess().Handle, -1, -1);
        }


        //private int maxWorkingSet = 750000;
        /// <summary>
        /// 设置程序占用最大内存
        /// </summary>
        ///
        public static void SetWorkingSet()//int maxWorkingSet)
        {
            int maxWorkingSet = 750000;
            System.Diagnostics.Process.GetCurrentProcess().MaxWorkingSet = (IntPtr)maxWorkingSet;
        }
    }
}
