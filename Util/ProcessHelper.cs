using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BD.Util
{
    public class ProcessHelper
    {
        /// <summary>
        /// 启动一个程序，附带参数，回调返回程序输出
        /// </summary>
        /// <param name="path">program</param>
        /// <param name="args">arguments</param>
        /// <param name="handler">callback</param>
        public static void ExecuteCmd(string path = "ping.exe", string args = "192.168.1.1", Action<string> handler = null)
        {
            System.Diagnostics.Process CmdProcess = new System.Diagnostics.Process();
            CmdProcess.StartInfo.FileName = path;      // 命令  
            CmdProcess.StartInfo.Arguments = args;      // 参数  

            CmdProcess.StartInfo.CreateNoWindow = true;         // 不创建新窗口  
            CmdProcess.StartInfo.UseShellExecute = false;
            CmdProcess.StartInfo.RedirectStandardInput = true;  // 重定向输入  
            CmdProcess.StartInfo.RedirectStandardOutput = true; // 重定向标准输出  
            CmdProcess.StartInfo.RedirectStandardError = true;  // 重定向错误输出  
            CmdProcess.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;  
            CmdProcess.OutputDataReceived += (s, e) => { handler?.Invoke(e.Data); };
            CmdProcess.EnableRaisingEvents = true;                      // 启用Exited事件  
            CmdProcess.Start();
            CmdProcess.BeginOutputReadLine();
            CmdProcess.BeginErrorReadLine();
        }
        /// <summary>
        /// 
        /// </summary>
        public enum NgenOption
        {
            /// <summary>
            /// 
            /// </summary>
            INSTALL,
            /// <summary>
            /// 
            /// </summary>
            UNINSTALL
        }
        /// <summary>
        /// 编译程序集生成本地映像
        /// </summary>
        /// <param name="ngenPath">ngen full path</param>
        /// <param name="assemblyPath">assembly full path</param>
        /// <param name="option"></param>
        public static void ExecuteNgen(string ngenPath,string assemblyPath,NgenOption option=NgenOption.INSTALL)
        {
            string arg = string.Format("install {0}",assemblyPath);
            if(option==NgenOption.UNINSTALL)
                arg= string.Format("uninstall {0}", assemblyPath);
            ExecuteCmd(ngenPath, arg);
        }
    }
}
