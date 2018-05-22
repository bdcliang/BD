using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace BD.InternalException
{
    public class ApplicationExceptionHandler
    {
        public static void Register()
        {
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += (s, e) => {
                e.GetType().log(e.Exception.Message);
            };
            AppDomain.CurrentDomain.UnhandledException += (s, e) => {
                System.Exception error = e.ExceptionObject as System.Exception;
                error.log(error.Message);
            };
        }
    }
}
