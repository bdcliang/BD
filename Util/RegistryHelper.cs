namespace Borui.WinAPI
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Microsoft.Win32;
    using Reg = Microsoft.Win32.Registry;
    /// <summary>
    /// 注册表操作
    /// </summary>
    public class RegistryHelper
    {
        /// <summary>
        /// 向指定的程序名下的key写值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="subkey"></param>
        /// <param name="value"></param>
        public static void RegAddKey(string key, string subkey, string value)
        {
            try
            {
                string arguments = string.Format(@"SOFTWARE\BDSOFT\{0}", key);
                RegistryKey reg = Reg.LocalMachine.OpenSubKey(arguments, true);
                if (null == reg) { reg = Reg.LocalMachine.CreateSubKey(arguments); }
                reg.SetValue(subkey, value); reg.Flush(); reg = null;
            }
            catch { return; }
        }
        /// <summary>
        /// 返回一个项下面的所有子项名称
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static List<string> RegGetAllSubKey(string key)
        {
            try
            {
                string arguments = string.Format(@"SOFTWARE\BDSOFT\{0}", key);
                Dictionary<string, int> dic = new Dictionary<string, int>();
                RegistryKey reg = Reg.LocalMachine.OpenSubKey(arguments, true);
                if (null == reg) { reg = Reg.LocalMachine.CreateSubKey(arguments); }
                return new List<string>(reg.GetSubKeyNames());

            }
            catch { return null; }
        }
        /// <summary>
        /// 从注册表中取出指定程序下的Key值
        /// </summary>
        /// <param name="key">程序ming</param>
        /// <param name="subkey">键</param>
        /// <returns></returns>
        public static string RegKeyRead(string key, string subkey)
        {
            try
            {
                string arguments = string.Format(@"SOFTWARE\BDSOFT\{0}", key);
                RegistryKey reg = Reg.LocalMachine.OpenSubKey(arguments, true);
                if (null == reg) { reg = Reg.LocalMachine.CreateSubKey(arguments); return ""; }
                return reg.GetValue(subkey) as string;
            }
            catch { return ""; }
        }

        /// <summary>
        /// 删除整个子项，@"SOFTWARE\BoRuiMultiMedia"
        /// </summary>
        /// <param name="key"></param>
        public static void RegRemoveKey(string key)
        {
            try
            {
                string arguments = string.Format(@"SOFTWARE\BDSOFT");
                RegistryKey reg = Reg.LocalMachine.OpenSubKey(arguments, true);
                if (null == reg) { return; }
                reg.DeleteSubKeyTree(key);
            }
            catch { return; }
        }
    }
}
