namespace BD.API
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Drawing;
    using Microsoft.Win32;
    using Reg = Microsoft.Win32.Registry;
   // using CoreAudioApi;



    [Flags]
    internal enum MouseEventFlag : uint
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
    internal struct TokPriv1Luid
    {
        public int Count;
        public long Luid;
        public int Attr;
    }

    internal enum ExitWindowsFlags:int
    {
        EWX_LOGOFF = 0x00000000,
        EWX_SHUTDOWN = 0x00000001,
        EWX_REBOOT = 0x00000002,
        EWX_FORCE = 0x00000004,
        EWX_POWEROFF = 0x00000008,
        EWX_FORCEIFHUNG = 0x00000010
    }

    internal enum WindowStyle:int
    {
        WS_EX_TOOLWINDOW = 0x80,
        GWL_EXSTYLE = -20,
        HWND_TOPMOST = -1,
        SWP_NOSIZE = 0x0001,
        SWP_NOMOVE = 0x0002
    }

    ///定义了辅助键的名称（将数字转变为字符以便于记忆，也可去除此枚举而直接使用数值）
    [Flags()]
    public enum KeyModifiers
    {
        None = 0,
        Alt = 1,
        Ctrl = 2,
        Shift = 4,
        WindowsKey = 8
    }

    class APIBase
    {
        #region dll imports
        [DllImport("user32.dll", EntryPoint = "FindWindowEx", SetLastError = true)]
        internal static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

        [DllImport("user32.dll", EntryPoint = "ShowWindow", SetLastError = true)]
        internal static extern bool ShowWindow(IntPtr hWnd, uint nCmdShow);

        [DllImport("User32.dll", EntryPoint = "SetWindowLong", SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
        internal static extern bool SetWindowLong(IntPtr hWnd, int nIndex, int nFlags);

        [DllImport("User32.dll", EntryPoint = "SetLayeredWindowAttributes", SetLastError = true)]
        internal static extern int GetWindowLong(IntPtr hWnd, int nFlags);

        [DllImport("User32.dll", EntryPoint = "SetWindowPos", SetLastError = true)]
        internal static extern int SetWindowPos(IntPtr hWnd, IntPtr hWndWinInsertAfter, int x, int y, int cx, int cy, int nFlags);

        [DllImport("User32.dll", EntryPoint = "FindWindow", CharSet = CharSet.Auto)]
        internal static extern int FindWindow(String className, String captionName);

        [DllImport("User32.dll", EntryPoint = "ShowWindow", CharSet = CharSet.Auto)]
        internal static extern int ShowWindow(int hwnd, int nCmdShow);

        [DllImport("user32.dll", EntryPoint = "ShowCursor", CharSet = CharSet.Auto)]
        internal static extern void ShowCursor(int status);

        [DllImport("user32.dll")]
        internal static extern bool GetCursorPos(ref Point lpPoint);

        [DllImport("user32.dll")]
        internal static extern bool SetCursorPos(int X, int Y);

        [DllImport("user32.dll")]
        internal static extern void mouse_event(MouseEventFlag flags, int dx, int dy, int data, UIntPtr extraInfo);

        [DllImport("kernel32.dll", ExactSpelling = true)]
        internal static extern IntPtr GetCurrentProcess();

        [DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
        internal static extern bool OpenProcessToken(IntPtr h, int acc, ref IntPtr phtok);

        [DllImport("advapi32.dll", SetLastError = true)]
        internal static extern bool LookupPrivilegeValue(string host, string name, ref long pluid);

        [DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
        internal static extern bool AdjustTokenPrivileges(IntPtr htok, bool disall,
            ref TokPriv1Luid newst, int len, IntPtr prev, IntPtr relen);

        [DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
        internal static extern bool ExitWindowsEx(int flg, int rea);


        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool RegisterHotKey(
           IntPtr hWnd,                //要定义热键的窗口的句柄
           int id,                     //定义热键ID（不能与其它ID重复）           
           KeyModifiers fsModifiers,   //标识热键是否在按Alt、Ctrl、Shift、Windows等键时才会生效
           System.Windows.Forms.Keys vk//定义热键的内容
           );

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool UnregisterHotKey(
            IntPtr hWnd,                //要取消热键的窗口的句柄
            int id                      //要取消热键的ID
            );
        #endregion

        internal const int SE_PRIVILEGE_ENABLED = 0x00000002;
        internal const int TOKEN_QUERY = 0x00000008;
        internal const int TOKEN_ADJUST_PRIVILEGES = 0x00000020;
        internal const string SE_SHUTDOWN_NAME = "SeShutdownPrivilege";



        //internal static MMDevice defaultDevice = null;
        static APIBase()
        {
           // MMDeviceEnumerator devEnum = new MMDeviceEnumerator();
           // defaultDevice = devEnum.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eMultimedia);
        }
        internal static void DoExitWin(int flg)
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
            if (!ExitWindowsEx(flg, 0))
            {
                throw new Exception("Exit Windows fail");
            }
        }
        
    }
}
