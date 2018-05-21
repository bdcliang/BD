using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BD.API
{
    public class Boot
    {
        /// <summary>
        /// reboot the system
        /// </summary>
        /// <param name="force"></param>
        public static void Reboot(bool force)
        {
            if (force)
                APIBase.DoExitWin((int)ExitWindowsFlags.EWX_REBOOT | (int)ExitWindowsFlags.EWX_FORCE);
            else
                APIBase.DoExitWin((int)ExitWindowsFlags.EWX_REBOOT | (int)ExitWindowsFlags.EWX_FORCEIFHUNG);
        }
        /// <summary>
        /// shutdown the system
        /// </summary>
        /// <param name="force"></param>
        public static void Shutdown(bool force)
        {
            if (force)
                APIBase.DoExitWin((int)ExitWindowsFlags.EWX_SHUTDOWN | (int)ExitWindowsFlags.EWX_FORCE);
            else
                APIBase.DoExitWin((int)ExitWindowsFlags.EWX_SHUTDOWN | (int)ExitWindowsFlags.EWX_FORCEIFHUNG);
        }
        /// <summary>
        /// logoff 
        /// </summary>
        /// <param name="force"></param>
        public static void Logoff(bool force)
        {
            if (force)
                APIBase.DoExitWin((int)ExitWindowsFlags.EWX_LOGOFF | (int)ExitWindowsFlags.EWX_FORCE);
            else
                APIBase.DoExitWin((int)ExitWindowsFlags.EWX_LOGOFF | (int)ExitWindowsFlags.EWX_FORCEIFHUNG);
        }
    }
}
