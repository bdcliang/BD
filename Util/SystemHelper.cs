namespace BD.Util
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Drawing;
    using Microsoft.Win32;
    using Reg = Microsoft.Win32.Registry;
    using CoreAudioApi;

    /// <summary>
    /// 系统启动，开关机，音量，鼠标，任务栏隐藏，窗口置顶
    /// </summary>
    public  class SystemHelper
    {
        #region dll imports
        [DllImport("user32.dll", EntryPoint = "FindWindowEx", SetLastError = true)]
        private static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

        [DllImport("user32.dll", EntryPoint = "ShowWindow", SetLastError = true)]
        private static extern bool ShowWindow(IntPtr hWnd, uint nCmdShow);

        [DllImport("User32.dll", EntryPoint = "SetWindowLong", SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool SetWindowLong(IntPtr hWnd, int nIndex, int nFlags);
        [DllImport("User32.dll", EntryPoint = "SetLayeredWindowAttributes", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nFlags);
        [DllImport("User32.dll", EntryPoint = "SetWindowPos", SetLastError = true)]
        private static extern int SetWindowPos(IntPtr hWnd, IntPtr hWndWinInsertAfter, int x, int y, int cx, int cy, int nFlags);

        const int WS_EX_TOOLWINDOW = 0x80;
        const int GWL_EXSTYLE = -20;
        const int HWND_TOPMOST = -1;
        const int SWP_NOSIZE = 0x0001;
        const int SWP_NOMOVE = 0x0002;

        [DllImport("User32.dll", EntryPoint = "FindWindow", CharSet = CharSet.Auto)]
        private static extern int FindWindow(String className, String captionName);
        [DllImport("User32.dll", EntryPoint = "ShowWindow", CharSet = CharSet.Auto)]
        private static extern int ShowWindow(int hwnd, int nCmdShow);
        [DllImport("user32.dll", EntryPoint = "ShowCursor", CharSet = CharSet.Auto)]
        private static extern void ShowCursor(int status);

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(ref Point lpPoint);

        [DllImport("user32.dll")]
        private static extern bool SetCursorPos(int X, int Y);

        [DllImport("user32.dll")]
        private static extern void mouse_event(MouseEventFlag flags, int dx, int dy, uint data, UIntPtr extraInfo);
        [Flags]
        enum MouseEventFlag : uint
        {
            Move = 0x0001,
            LeftDown = 0x0002,
            LeftUp = 0x0004,
            RightDown = 0x0008,
            RightUp = 0x0010,
            MiddleDown = 0x0020,
            MiddleUp = 0x0040,
            XDown = 0x0080,
            XUp = 0x0100,
            Wheel = 0x0800,
            VirtualDesk = 0x4000,
            Absolute = 0x8000
        }
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct TokPriv1Luid
        {
            public int Count;
            public long Luid;
            public int Attr;
        }

        [DllImport("kernel32.dll", ExactSpelling = true)]
        private static extern IntPtr GetCurrentProcess();
        [DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
        private static extern bool OpenProcessToken(IntPtr h, int acc, ref IntPtr phtok);
        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool LookupPrivilegeValue(string host, string name, ref long pluid);

        [DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
        private static extern bool AdjustTokenPrivileges(IntPtr htok, bool disall,
            ref TokPriv1Luid newst, int len, IntPtr prev, IntPtr relen);

        [DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
        private static extern bool ExitWindowsEx(int flg, int rea);
        private const int SE_PRIVILEGE_ENABLED = 0x00000002;


        private const int TOKEN_QUERY = 0x00000008;
        private const int TOKEN_ADJUST_PRIVILEGES = 0x00000020;
        private const string SE_SHUTDOWN_NAME = "SeShutdownPrivilege";

        #region Exit Windows Flags
        private const int EWX_LOGOFF = 0x00000000;
        private const int EWX_SHUTDOWN = 0x00000001;
        private const int EWX_REBOOT = 0x00000002;
        private const int EWX_FORCE = 0x00000004;
        private const int EWX_POWEROFF = 0x00000008;
        private const int EWX_FORCEIFHUNG = 0x00000010;
        #endregion
        #endregion
        /// <summary>
        /// 隐藏任务栏
        /// </summary>
        /// <param name="Hide">是否隐藏</param>
        public static void HideTask(bool Hide=true)
        {
            try
            {
                IntPtr trayHwnd = FindWindowEx(IntPtr.Zero, IntPtr.Zero, "Shell_TrayWnd", null);
                IntPtr hStar = FindWindowEx(IntPtr.Zero, IntPtr.Zero, "Button", null);
                if (Hide)
                {
                    ShowWindow(trayHwnd, 0);
                    ShowWindow(hStar, 0);
                }
                else
                {
                    ShowWindow(trayHwnd, 1);
                    ShowWindow(hStar, 1);
                }
            }
            catch { return; }
        }

        
        /// <summary>
        /// 将 窗口置顶
        /// </summary>
        /// <param name="windowHandle"></param>
        public static void SetWindowAlwaysTop(IntPtr windowHandle)
        {
            SetWindowLong(windowHandle, GWL_EXSTYLE, GetWindowLong(windowHandle, GWL_EXSTYLE) | WS_EX_TOOLWINDOW);
            SetWindowPos(windowHandle, (IntPtr)HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);
        }

        
        public enum CursorOption
        {
            HIDDEN,
            SHOW
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="option"></param>
        public static void SetCursorStatus(CursorOption option=CursorOption.HIDDEN)
        {
            if (option==CursorOption.HIDDEN) ShowCursor(0);
            else ShowCursor(1);
        }

        /// <summary>
        /// 获取鼠标位置
        /// </summary>
        /// <returns></returns>
        public static Point GetCursorPos()
        {
            Point point = new Point();
            GetCursorPos(ref point);
            return point;
        }
        /// <summary>
        /// 设置鼠标的位置
        /// </summary>
        /// <param name="p"></param>
        public static void SetCursorPos(Point p)
        {
            SetCursorPos(p.X, p.Y);
        }
        /// <summary>
        /// 模拟鼠标点击效果
        /// </summary>
        public static void MouseClick()
        {
            mouse_event(MouseEventFlag.LeftDown, 0, 0, 0, UIntPtr.Zero);
            mouse_event(MouseEventFlag.LeftUp, 0, 0, 0, UIntPtr.Zero);
        }
        /// <summary>
        /// 鼠标左键按下
        /// </summary>
        public static void MouseLeftDown()
        {
            mouse_event(MouseEventFlag.LeftDown, 0, 0, 0, UIntPtr.Zero);
        }
        /// <summary>
        /// 鼠标左键释放
        /// </summary>
        public static void MouseLeftUp()
        {
            mouse_event(MouseEventFlag.LeftUp, 0, 0, 0, UIntPtr.Zero);
        }

        /// <summary>
        /// 
        /// </summary>
        public enum AutoRunOption
        {
            /// <summary>
            /// 
            /// </summary>
            SET,
            /// <summary>
            /// 
            /// </summary>
            CANCEL
        }

        /// <summary>
        /// Set programm run after the system boot
        /// </summary>
        /// <param name="filepath"></param>
        /// <param name="option"></param>
        public static void SetAppAutoRun(string filepath, AutoRunOption option=AutoRunOption.SET)
        {
            try
            {
                RegistryKey reg = null;
                reg = Reg.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
                if (reg == null)
                    reg = Reg.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run");
                if (!System.IO.File.Exists(filepath)) return;
                String name = Path.GetFileNameWithoutExtension(filepath);
                if (option==AutoRunOption.SET) { if (null != reg.GetValue(name)) return; reg.SetValue(name, filepath); System.Windows.Forms.MessageBox.Show("设置自启动成功"); }
                else { reg.DeleteValue(name); System.Windows.Forms.MessageBox.Show("取消开机启动成功"); }
                reg.Flush();
                reg = null;
            }
            catch {  return; }
        }

        /// <summary>
        /// 检查文件是否已设置为开机启动
        /// </summary>
        /// <param name="filepath"></param>
        /// <returns></returns>
        public static bool IsAutoRun(string filepath)
        {
            try
            {
                RegistryKey reg = null;
                reg = Reg.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
                if (reg == null) return false;                    
                if (!System.IO.File.Exists(filepath)) return false;
                String name = Path.GetFileNameWithoutExtension(filepath);
                if (null != reg.GetValue(name)) return true; 
                reg = null;
                return false;
            }
            catch { return false; }
        }


        private static MMDevice defaultDevice = null;
        static SystemHelper()
        {
            MMDeviceEnumerator devEnum = new MMDeviceEnumerator();
            defaultDevice = devEnum.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eMultimedia);
        }

        #region  系统音量操作
        //
        /// <summary>
        /// 判断当前系统扬声器状态
        /// </summary>
        /// <returns></returns>
        public static bool IsMuted()
        {
            return defaultDevice.AudioEndpointVolume.Mute;
        }
        //
        /// <summary>
        /// 静音
        /// </summary>
        public static void SetMute()
        {
            defaultDevice.AudioEndpointVolume.Mute = true;
        }
        // 
        /// <summary>
        /// 解除静音
        /// </summary>
        public static void SetUnMute()
        {
            defaultDevice.AudioEndpointVolume.Mute = false;
        }
        #endregion



        #region   系统开关机操作
        private static void DoExitWin(int flg)
        {
            TokPriv1Luid tp;
            IntPtr hproc = GetCurrentProcess();
            IntPtr htok = IntPtr.Zero;
            if (!OpenProcessToken(hproc, TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, ref htok))
            {
                throw new Exception("Open Process Token fail");
            }
            tp.Count = 1;
            tp.Luid = 0;
            tp.Attr = SE_PRIVILEGE_ENABLED;
            if (!LookupPrivilegeValue(null, SE_SHUTDOWN_NAME, ref tp.Luid))
            {
                throw new Exception("Lookup Privilege Value fail");
            }
            if (!AdjustTokenPrivileges(htok, false, ref tp, 0, IntPtr.Zero, IntPtr.Zero))
            {
                throw new Exception("Adjust Token Privileges fail");
            }
            //Exit windows
            if (!ExitWindowsEx(flg, 0))
            {
                throw new Exception("Exit Windows fail");
            }
        }
        /// <summary>
        /// 重启
        /// </summary>
        /// <param name="force"></param>
        public static void Reboot(bool force)
        {
            if (force)
                DoExitWin(EWX_REBOOT | EWX_FORCE);
            else
                DoExitWin(EWX_REBOOT | EWX_FORCEIFHUNG);
        }
        /// <summary>
        /// 关机
        /// </summary>
        /// <param name="force"></param>
        public static void Shutdown(bool force)
        {
            if (force)
                DoExitWin(EWX_SHUTDOWN | EWX_FORCE);
            else
                DoExitWin(EWX_SHUTDOWN | EWX_FORCEIFHUNG);
        }
        /// <summary>
        /// logoff the computer
        /// </summary>
        /// <param name="force"></param>
        public static void Logoff(bool force)
        {
            if (force)
                DoExitWin(EWX_LOGOFF | EWX_FORCE);
            else
                DoExitWin(EWX_LOGOFF | EWX_FORCEIFHUNG);
        }
        #endregion
    }
}

