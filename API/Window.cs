using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BD.API
{
    public class Window
    {
        /// <summary>
        /// show the start button and the toolbars
        /// </summary>
        public static void ShowToolbars()
        {
            IntPtr trayHwnd = APIBase.FindWindowEx(IntPtr.Zero, IntPtr.Zero, "Shell_TrayWnd", null);
            IntPtr hStar = APIBase.FindWindowEx(IntPtr.Zero, IntPtr.Zero, "Button", null);
            APIBase.ShowWindow(trayHwnd, 1);
            APIBase.ShowWindow(hStar, 1);
        }

        /// <summary>
        /// hidde the start button and the toolbars
        /// </summary>
        public static void HiddeToolbars()
        {
            IntPtr trayHwnd = APIBase.FindWindowEx(IntPtr.Zero, IntPtr.Zero, "Shell_TrayWnd", null);
            IntPtr hStar = APIBase.FindWindowEx(IntPtr.Zero, IntPtr.Zero, "Button", null);
            APIBase.ShowWindow(trayHwnd, 0);
            APIBase.ShowWindow(hStar, 0);
        }

        /// <summary>
        /// set the window topmost
        /// </summary>
        /// <param name="windowHandle"></param>
        public static void TopMost(IntPtr windowHandle)
        {           
            APIBase.SetWindowLong(windowHandle,(int)WindowStyle.GWL_EXSTYLE, APIBase.GetWindowLong(windowHandle, (int)WindowStyle.GWL_EXSTYLE) | (int)WindowStyle.WS_EX_TOOLWINDOW);
            APIBase.SetWindowPos(windowHandle, (IntPtr)WindowStyle.HWND_TOPMOST, 0, 0, 0, 0, (int)WindowStyle.SWP_NOMOVE | (int)WindowStyle.SWP_NOSIZE);
        }
        /// <summary>
        /// set the window topmost
        /// </summary>
        /// <param name="control"></param>
        public static void TopMost(System.Windows.Forms.Control control)
        {
            IntPtr handle = control.Handle;
            TopMost(handle);
        }

        /// <summary>
        /// set the given file like .exe .file run with the system boot
        /// </summary>
        /// <param name="filepath"></param>
        /// <returns></returns>
        public static bool SetAutorun(string filepath)
        {
            try
            {
                RegistryKey reg =Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
                if (reg == null)
                    reg = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run");
                if (!System.IO.File.Exists(filepath)) return false;
                String name = Path.GetFileNameWithoutExtension(filepath);
                if (null != reg.GetValue(name)) return true;
                reg.SetValue(name, filepath);                
                reg.Flush();
                reg = null;
                return true;
            }
            catch { return false; }
        }

        /// <summary>
        /// cancel the autorun files
        /// </summary>
        /// <param name="filepath"></param>
        /// <returns></returns>
        public static bool CancelAutorun(string filepath)
        {
            try
            {
                RegistryKey reg = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
                if (reg == null)
                    reg = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run");
                String name = Path.GetFileNameWithoutExtension(filepath);
                if (null != reg.GetValue(name))
                  reg.DeleteValue(name);
                reg.Flush();
                reg = null;
                return true;
            }
            catch { return false; }
        }

        /// <summary>
        /// to check an application if it is setted to autorun
        /// </summary>
        /// <returns></returns>
        public static bool CheckAutorun(string filepath)
        {
            try
            {
                RegistryKey reg =Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
                if (reg == null) return false;
                if (!System.IO.File.Exists(filepath)) return false;
                String name = Path.GetFileNameWithoutExtension(filepath);
                if (null != reg.GetValue(name)) return true;
                reg = null;
                return false;
            }
            catch { return false; }
        }

        public static void OnlyProcessShow(System.Windows.Forms.Form form)
        {
            form.Hide();
            form.WindowState = System.Windows.Forms.FormWindowState.Minimized;
            form.ShowInTaskbar = false;
            form.Text = "";
        }

    }
}
