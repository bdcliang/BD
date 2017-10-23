using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BD.IO
{
    public class LogHelper
    {
        /// <summary>
        /// Write log
        /// </summary>
        /// <param name="msg"></param>
        public static void log(string msg)
        {
            string path = Environment.CurrentDirectory + @"\log.txt";
            String strbuilder = "";
            strbuilder += DateTime.Now.ToString("yyyy-MM-dd  hh:mm:ss");
            strbuilder += "  ";
            strbuilder += msg;
            var stream = File.AppendText(path);
            stream.Write(strbuilder);
            stream.WriteLine();
            stream.Flush();
            stream.Close();
        }
    }
}
