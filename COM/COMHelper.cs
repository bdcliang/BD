namespace BD.COM
{
    using Microsoft.Win32;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;
    public class COMHelper
    {
        /// <summary>
        /// get the specify  com status if is resigtered,clsid without blocks
        /// </summary>
        /// <param name="clsid"></param>
        /// <returns></returns>
        public static bool IsComRegistered(string clsid)
        {
            string cld = String.Format("\\CLSID\\{0}{1}{2}", "{", clsid, "}");
            RegistryKey comkey = Registry.ClassesRoot.OpenSubKey(cld);
            //RegistryKey root = RegistryKey.OpenBaseKey(RegistryHive.ClassesRoot, RegistryView.Registry32);
            
            //RegistryKey comKey = root.OpenSubKey(cld);
            if (comkey != null)
                return true;
            return false;
        }
        /// <summary>
        /// register the specify com component
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="register"></param>
        /// <returns></returns>
        private static bool Register(string filename, bool register)
        {
            if (!File.Exists(filename)) { return false; }
            Process p = new Process();
            try {
            p.StartInfo.FileName = string.Format("{0}\\{1}", Environment.GetFolderPath(Environment.SpecialFolder.System),
                "regsvr32.exe");
            p.StartInfo.Arguments = string.Format("/s {0}", filename);
            if (!register)
            {
                p.StartInfo.Arguments += " /u";
            }
            p.Start();
            }
            catch (Exception e) { typeof(COMHelper).log(true,e.Message); return false; }
            return true;
        }
    }
}
