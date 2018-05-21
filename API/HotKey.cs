using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BD.API
{
    public class HotKey
    {
        public static void WndProc(ref System.Windows.Forms.Message m,Action<int> continueWith=null)
        {
            const int WM_HOTKEY = 0x0312;
            switch (m.Msg)
            {
                case WM_HOTKEY:
                    continueWith?.Invoke(m.WParam.ToInt32());                 
                    break;
            }
        }
        /// <summary>
        /// register hotkey
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="id"></param>
        /// <param name="fsModifiers"></param>
        /// <param name="vk"></param>
        public static bool RegisterHotKey(IntPtr hWnd,int id,KeyModifiers fsModifiers,System.Windows.Forms.Keys vk)
        {
            return APIBase.RegisterHotKey(hWnd, id, fsModifiers, vk);
        }
        /// <summary>
        /// unregister hotkey
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static  bool UnregisterHotKey(IntPtr hWnd,int id)
        {
            return APIBase.UnregisterHotKey(hWnd, id);
        }
    }
}
