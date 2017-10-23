namespace BD.IO
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    /// <summary>
    /// a File Helper to simply the file operation
    /// </summary>
    public class FileHelper
    {       
        /// <summary>
        /// 新建文件
        /// </summary>
        /// <param name="filePath"></param>
        public static void CreateFile(string filePath)
        {
            if (!File.Exists(filePath))
                File.Create(filePath);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rootpath"></param>
        /// <param name="serachpattern">搜索模式like:*jpg|*exe|*txt</param>
        /// <returns></returns>
        public static List<string> SearchFiles(string rootpath, string serachpattern)
        {
            if (!Directory.Exists(rootpath)) return null;
            string[] fileTypes=fileTypes = serachpattern.Split('|', ',', ' ');
            DirectoryInfo dirinfo = new DirectoryInfo(rootpath);
            List<FileInfo> list = new List<FileInfo>();
            foreach (var pattern in fileTypes)
            {
                var tmp = dirinfo.GetFiles(pattern, SearchOption.AllDirectories);
                list.AddRange(tmp);
            }
            return list.ConvertAll(m => m.FullName);
        }
    }
}
